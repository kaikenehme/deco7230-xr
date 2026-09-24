/// <summary>
/// A diegetic prop you touch with a controller (preset frame, lamp, clock). On device the
/// controller cue's trigger collider calls Touch; in the editor's desktop mode a click on it does.
/// </summary>
public interface ITouchable
{
    void Touch();
}
