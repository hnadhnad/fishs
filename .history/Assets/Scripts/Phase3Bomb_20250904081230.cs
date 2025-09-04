using UnityEngine;

public class Phase3Bomb : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.gameObject.GetComponent<Fish>();
        if (player != null && player.isPlayer)
        {
            player.Die(); // player chạm bomb thì chết
            return;
        }

        var boss = collision.gameObject.GetComponent<Boss>();
        if (boss != null)
        {
            // Boss đâm trúng bomb
            boss.Stun(boss.phase3BombStunDuration);

            // Spawn thịt bên trong vòng
            if (boss.currentState is BossPhase3State state)
            {
                state.SpawnMeatOnBombHit(boss);
            }
        }
    }
}
