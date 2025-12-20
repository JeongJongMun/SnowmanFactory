using System;
using System.Linq;
using Core.Scripts.Table;
using UnityEngine;

public struct MissionData
{
    public float time;
    public float amount;
}

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject snowmanPrefab = null; 
        
    [SerializeField] private Animator animatorSnowmanUpperbody = null;
    
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private Transform arrivalPoint = null;
    
    [SerializeField] private GimmickPress gimmickPress = null;
    
    private float _currentSnowmanMoveDuration = 3.0f;
    private float _railAnimationPlayRate = 1.0f;
    private Snowman _currentSnowman = null;

    public event Action OnSnowmanSpawnEvent = null;
    public event Action OnSnowmanArrivalEvent = null;
    public event Action OnSnowmanDeathEvent = null; 
    
    public void Kill()
    {
        if (_currentSnowman == null) return;
        
        // TODO : Random Gimmick Activation
        gimmickPress.Activate();
    }

    public void Pass()
    {
        _currentSnowman.Stop();
        _currentSnowman.MoveImmediate();
    }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        SpawnSnowman();
    }

    private void Init()
    {
        _railAnimationPlayRate = 1.0f;
        
        gimmickPress.Init();
        gimmickPress.OnStartActivateEvent += () =>
        {
            _currentSnowman.Stop();
        };
        gimmickPress.OnFinishActivateEvent += () =>
        {
            OnSnowmanDeathEvent?.Invoke();
            Destroy(_currentSnowman.gameObject);
            animatorSnowmanUpperbody.runtimeAnimatorController = null;
        };
        gimmickPress.OnStartDeactivateEvent += SpawnSnowman;
    }

    private void Release()
    {
        
    }
    
    private void CreateRandomMission()
    {
        
    }

    private void SpawnSnowman()
    {
        var idCount = Tables.Instance.GetTable<SnowmanTd>().GetKeys.Count();
        var id = UnityEngine.Random.Range(0, idCount);
        var data = Tables.Instance.GetTable<SnowmanTd>().GetData(id);
        
        var path = $"Animations/Snowman/{data.upperbody_animator_name}/{data.upperbody_animator_name}";
        var controller = Resources.Load<RuntimeAnimatorController>(path);
        if (controller != null)
        {
            animatorSnowmanUpperbody.runtimeAnimatorController = controller;
        }
        
        _currentSnowman = Instantiate(snowmanPrefab, spawnPoint).GetComponent<Snowman>();
        _currentSnowman.Init(data);
        
        var movementData = new MovementData
        {
            targetPosition = arrivalPoint.position,
            duration = _currentSnowmanMoveDuration,
            doneCallback = () =>
            {
                OnSnowmanArrivalEvent?.Invoke();
                SpawnSnowman();
            }
        };
        _currentSnowman.Move(movementData);
    }

    private void OnDrawGizmos() 
    {
        if (spawnPoint == null || arrivalPoint == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPoint.position, 30.0f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(arrivalPoint.position, 30.0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(spawnPoint.position, arrivalPoint.position);
    }
}