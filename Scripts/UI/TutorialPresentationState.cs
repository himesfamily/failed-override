namespace FailedOverride.UI;

public sealed class TutorialPresentationState
{
    public string LessonId { get; private set; } = "";
    public bool IsVisible { get; private set; }

    public void Show(string lessonId)
    {
        LessonId = lessonId;
        IsVisible = true;
    }

    public void Dismiss()
    {
        IsVisible = false;
    }

    public void Complete()
    {
        LessonId = "";
        IsVisible = false;
    }

    public void Reset()
    {
        LessonId = "";
        IsVisible = false;
    }
}
