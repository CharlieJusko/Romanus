public interface IPlayerAction
{
    public bool CanCommit { get; set; }
    public bool InAction { get; set; }

    public void Initialize();
    public void Reset();
    public void Scan();
    public void Update();

}
