using Kepware.Api.Serializer;
using System.Security.AccessControl;
using System.Text.Json;

namespace Kepware.Api.Model.Services
{
    public class KepServerJobPromise : IDisposable
    {
        private readonly Task<ApiResponse<bool>> m_completionTask;
        private readonly CancellationTokenSource? m_cancellationTokenSource;
        private bool m_hasBeenDisposed = false;

        public string Endpoint { get; }
        public TimeSpan JobTimeToLive { get; }

        internal KepServerJobPromise(string endpoint, TimeSpan timeout, JobResponseMessage jobCreationResponseMsg, HttpClient httpClient)
        {
            Endpoint = endpoint;
            JobTimeToLive = timeout;

            if (jobCreationResponseMsg.ResponseStatus == ApiResponseCode.Accepted && jobCreationResponseMsg.JobId != null)
            {
                m_cancellationTokenSource = new();
                m_completionTask = AwaitCompletionAsync(jobCreationResponseMsg.JobId, timeout, httpClient, m_cancellationTokenSource.Token);
            }
            else
            {
                m_completionTask = Task.FromResult(new ApiResponse<bool>(jobCreationResponseMsg.ResponseStatus, jobCreationResponseMsg.Message ?? "Job was not accepted", endpoint));
            }
        }

        internal KepServerJobPromise(string endpoint, TimeSpan timeout, ApiResponseCode responseCode, string message)
        {
            Endpoint = endpoint;
            JobTimeToLive = timeout;
            m_completionTask = Task.FromResult(new ApiResponse<bool>(responseCode, message, endpoint));
        }


        public async Task<ApiResponse<bool>> AwaitCompletionAsync(CancellationToken cancellationToken = default)
        {
            if (!m_hasBeenDisposed)
            {
                cancellationToken.Register(() => m_cancellationTokenSource?.Cancel());
                return await m_completionTask.ConfigureAwait(false);
            }
            else
            {
                throw new ObjectDisposedException(nameof(KepServerJobPromise));
            }
        }

        private static async Task<ApiResponse<bool>> AwaitCompletionAsync(string jobHref, TimeSpan timeout, HttpClient httpClient, CancellationToken cancellationToken)
        {
            using var timeoutCts = new CancellationTokenSource(timeout);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            TaskCanceledException? taskCanceledException = null;

            try
            {
                while (!linkedCts.Token.IsCancellationRequested)
                {
                    var response = await httpClient.GetAsync(jobHref, linkedCts.Token).ConfigureAwait(false);
                    var message = await response.Content.ReadAsStringAsync(linkedCts.Token).ConfigureAwait(false);

                    var jobStatus = JsonSerializer.Deserialize<JobStatusMessage>(message, KepJsonContext.Default.JobStatusMessage);

                    if (jobStatus?.Completed == true)
                    {
                        if (string.IsNullOrEmpty(jobStatus.Message))
                        {
                            //if the job is completed without a message, it is a success
                            return new ApiResponse<bool>(ApiResponseCode.Success, true, jobHref);
                        }
                        else
                        {
                            return new ApiResponse<bool>((ApiResponseCode)(int)response.StatusCode, jobStatus.Message, jobHref);
                        }
                    }
                    else if (!linkedCts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(250, linkedCts.Token).ConfigureAwait(false); // Poll every 0.25 second
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                // operation has been cancled
                taskCanceledException = tce;
            }

            if (timeoutCts.Token.IsCancellationRequested)
            {
                return new ApiResponse<bool>(ApiResponseCode.Timeout, "Timeout whaiting for the job completition", jobHref);
            }
            else
            {
                throw (taskCanceledException ?? new TaskCanceledException());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_hasBeenDisposed)
            {
                if (disposing)
                {
                    m_cancellationTokenSource?.Dispose();
                    m_completionTask.Dispose();
                }
                m_hasBeenDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
