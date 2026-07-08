using System;
using SyncVerse.Application.Common.Results;

namespace SyncVerse.Application.Interfaces.AI.Echo
{
    public interface IAiBulkSyncService
    {
        // 🎯 كتابة المسار الكامل للـ Task لمنع الالتباس مع موديل الـ Task
        System.Threading.Tasks.Task SyncSingleChangeToEchoAsync(
            Guid projectId,
            string title,
            string content,
            string type,
            string teamName = "");

        System.Threading.Tasks.Task<Result<int>> SyncAllApplicationDataToEchoAsync(Guid projectId);
    }
}