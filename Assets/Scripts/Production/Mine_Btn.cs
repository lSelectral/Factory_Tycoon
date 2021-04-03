public class Mine_Btn : ProductionBase
{
    public ScriptableMine scriptableMine;

    protected override void Start()
    {
        base.Start();
        workModeBtn.onClick.AddListener(() => ChangeWorkingMode(true));
    }

    #region Event Methods

    #endregion
}