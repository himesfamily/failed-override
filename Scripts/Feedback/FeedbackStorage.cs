using Godot;

namespace FailedOverride.Feedback;

public static class FeedbackStorage
{
    const string DirectoryPath = "user://feedback";

    public static string Format(string type, string subject, string message, string context)
    {
        var title = string.IsNullOrWhiteSpace(subject) ? "(No subject)" : subject.Trim();
        return $"FAILED OVERRIDE PLAYTEST FEEDBACK\n\nType: {type}\nSubject: {title}\nSubmitted: {DateTime.Now:yyyy-MM-dd HH:mm}\nContext: {context}\n\n{message.Trim()}\n";
    }

    public static string Save(string type, string subject, string message, string context)
    {
        var absoluteDirectory = ProjectSettings.GlobalizePath(DirectoryPath);
        DirAccess.MakeDirRecursiveAbsolute(absoluteDirectory);
        var filename = $"feedback-{DateTime.Now:yyyyMMdd-HHmmss-fff}.txt";
        var path = $"{DirectoryPath}/{filename}";
        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
        file.StoreString(Format(type, subject, message, context));
        return ProjectSettings.GlobalizePath(path);
    }

    public static string AbsoluteDirectory()
    {
        var path = ProjectSettings.GlobalizePath(DirectoryPath);
        DirAccess.MakeDirRecursiveAbsolute(path);
        return path;
    }
}
