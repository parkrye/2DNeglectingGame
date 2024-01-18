using UnityEngine;

public class BTA_MoveToClose : BTAction
{
    private Transform _enemyTransform;

    public override bool Work()
    {
        var enemies = Global.CurrentStage.Enemies;
        if (enemies.Count == 0)
            return false;

        var player = Global.CurrentStage.PlayerActor;
        switch (_state)
        {
            case ActionState.Ready:
                _state = ActionState.Working;
                var closestDistance = float.MaxValue;
                var closestEnemy = enemies[0];

                foreach (var enemy in enemies)
                {
                    float distance = Vector3.SqrMagnitude(player.transform.position - enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }

                _enemyTransform = closestEnemy.transform;
                break;
            case ActionState.Working:
                if(Vector3.SqrMagnitude(player.transform.position - _enemyTransform.position) < 4f)
                {
                    _state = ActionState.End;
                    break;
                }
                player.gameObject.transform.LookAt(_enemyTransform);
                player.gameObject.transform.Translate((_enemyTransform.position - player.transform.position).normalized
                    * Global.UD.ActorData.MoveSpeed * 0.01f, Space.World);
                break;
            case ActionState.End:
                break;
        }

        return true;
    }
}
