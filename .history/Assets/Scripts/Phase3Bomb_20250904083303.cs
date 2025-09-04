using UnityEngine;

public class Phase3Bomb : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.gameObject.GetComponent<Fish>();
        if (player != null && player.isPlayer)
        {
            player.Die();
            return;
        }

        var boss = collision.gameObject.GetComponent<Boss>();
        if (boss != null)
        {
            // ✅ Boss bị choáng khi đâm trúng bomb
            boss.Stun(boss.phase3BombStunDuration);

            // ✅ Spawn thịt
            float healPerPiece = boss.phase3BombHitDamage / Mathf.Max(1, boss.phase3MeatCount);
            float step = 360f / boss.phase3MeatCount;

            for (int i = 0; i < boss.phase3MeatCount; i++)
            {
                float ang = step * i * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
                Vector3 spawnPos = boss.transform.position + dir * boss.phase3MeatSpawnOffset;

                GameObject piece = Instantiate(boss.meatPrefab, spawnPos, Quaternion.identity);
                if (!piece.TryGetComponent<MeatPiece>(out var mp))
                    mp = piece.AddComponent<MeatPiece>();

                mp.SetHealAmount(healPerPiece);
            }
        }
    }
}
