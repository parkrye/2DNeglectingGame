using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class EnemyActor : Actor, IHitable
{
    private ActorData _data;
    public ActorData Data { get { return _data; } }
    private int _hp;
    private EnemyActionHandler _actionHandler;
    private NavMeshAgent _navMesh;
    private NormalAnimationController _anim;
    public NormalAnimationController Anim { get { return _anim; } }

    public UnityEvent<EnemyActor> EnemyDie = new UnityEvent<EnemyActor>();
    public UnityEvent<float, bool> HPRatioEvent = new UnityEvent<float, bool>();

    private void Awake()
    {
        _actionHandler = GetComponent<EnemyActionHandler>();
        if (_actionHandler == null)
            _actionHandler = gameObject.AddComponent<EnemyActionHandler>();
        _anim = GetComponent<NormalAnimationController>();
        if( _anim == null )
            _anim = gameObject.AddComponent<NormalAnimationController>();
        var hpBar = GetComponentInChildren<HPBar>();
        if (hpBar != null)
            HPRatioEvent.AddListener(hpBar.ModifyBar);

        _type = ActorType.Enemy;
    }

    private void OnEnable()
    {
        if(_data == null)
            _data = Global.Datas.GetEnemyData(1);
        _hp = _data.Hp;
        _state = ActorState.Alive;
        if (_navMesh == null)
            _navMesh = gameObject.AddComponent<NavMeshAgent>();
        ActionRoutine().Forget();
        HPRatioEvent?.Invoke(1f, true);
    }

    private async UniTask ActionRoutine()
    {
        await UniTask.WaitUntil(() => _state == ActorState.Alive);

        while (_state == ActorState.Alive)
        {
            _actionHandler.Work();
            await UniTask.WaitForEndOfFrame();
        }
    }

    public bool Hit(float damage)
    {
        _hp -= (int)damage;
        HPRatioEvent?.Invoke((float)_hp / Data.Hp, true);
        if (_hp <= 0)
        {
            _state = ActorState.Dead;
            _anim.DieEndEvent.AddListener(DieReaction);
            _anim.PlayDieAnimation();
        }
        else
        {
            _anim.PlayHitAnimation();
        }
        return true;
    }

    private void DieReaction()
    {
        EnemyDie?.Invoke(this);
    }

    public void SetDestination(Vector3 position)
    {
        if (_navMesh == null)
            return;

        _navMesh.SetDestination(position);
    }

    public void LookAt(Vector3 position)
    {
        transform.LookAt(position);
    }
}
