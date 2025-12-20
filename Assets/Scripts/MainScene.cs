    using Core.Scripts;
using Core.Scripts.Manager;
using Core.Scripts.Table;
using DG.Tweening;
using UnityEngine;

public class MainScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

        Init();
    }
    
    public override void Init()
    {
        base.Init();
        
        // Debug.Log("MainScene initialized.");
        //
        // Managers.Toast.ShowToast("Hello, World!");
        // var popup = Managers.UI.ShowPopupUI<UIPopupExample>();
        //
        // DOVirtual.DelayedCall(1.0f, () =>
        // {
        //     Managers.UI.ClosePopupUI(popup);
        // });
        //
        // var data = Tables.Instance.GetTable<TestTable>().GetData(1001);
        // Debug.Log($" ID: {data.ID}, Name: {data.Name}, HP: {data.HP}, Damage: {data.Damage}");
    }
}
