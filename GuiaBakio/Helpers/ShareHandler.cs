public static class ShareHandler
{
    public static event Func<List<string>, Task>? OnFilesReceived;

    public static async Task ReceiveSharedFiles(List<string> filePaths)
    {
        if (OnFilesReceived != null)
        {
            foreach (var handler in OnFilesReceived.GetInvocationList())
            {
                await ((Func<List<string>, Task>)handler)(filePaths);
            }
        }
    }
}