using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Kaze
// ------------------------------------------------------------------
// A bot that follows a square movement pattern along the arena walls.
// It rotates near the walls and shifts inward and outward as it completes rotations.
// In normal mode, it fires with different bullet types (small, medium, large)
// based on the enemy's distance.
// ------------------------------------------------------------------
public class Kaze : Bot
{
    // Movement parameters for the square pattern.
    private readonly double margin = 50;           // Margin for shifting inward/outward.
    private double nearWallDistance;               // Side length when moving near the wall.
    private double innerDistance;                  // Side length when moving inside.
    private bool nearWall = true;                  // Current mode: true = near wall, false = inner.
    private int segmentCount = 0;                  // Counts segments in the current rotation.

    // Main method.
    static void Main(string[] args)
    {
        new Kaze().Start();
    }

    // Constructor: load bot configuration.
    Kaze() : base(BotInfo.FromFile("Kaze.json")) { }

    public override void Run()
    {
        // Set bot colors.
        BodyColor = Color.Blue;
        TurretColor = Color.DarkBlue;
        RadarColor = Color.Cyan;
        BulletColor = Color.White;
        ScanColor = Color.Cyan;

        // Compute movement distances based on the arena dimensions.
        nearWallDistance = Math.Min(ArenaWidth, ArenaHeight) - margin;
        innerDistance = nearWallDistance - 2 * margin;

        // Position the bot near the wall.
        TurnRight(Direction % 90);
        Forward(nearWallDistance);

        // Main loop.
        while (IsRunning)
        {
            // Normal mode: follow the square movement pattern.
            double currentDistance = nearWall ? nearWallDistance : innerDistance;
            Forward(currentDistance);
            TurnRight(90);
            segmentCount++;

            // After completing one full rotation (4 segments):
            if (segmentCount >= 4)
            {
                if (nearWall)
                {
                    // Completed near-wall rotation: shift inward.
                    TurnRight(90);
                    Forward(margin);
                    nearWall = false;
                }
                else
                {
                    // Completed inner rotation: reposition back near the wall.
                    TurnLeft(90);
                    Forward(margin);
                    nearWall = true;
                }
                segmentCount = 0;
            }
            // Continue scanning while moving.
            Rescan();
        }
    }

    // Event: When another bot is scanned.
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Hitung jarak ke musuh.
        double distance = DistanceTo(e.X, e.Y);
        double bulletPower;

        // Greedy bullet selection based on jarak:
        // - Jika sangat dekat (<150), gunakan peluru besar.
        // - Jika jauh (>250), gunakan peluru kecil.
        // - Lainnya, gunakan peluru sedang.

        if (distance < 150)
        {
            bulletPower = 3; // Peluru besar.
        }
        else if (distance > 250)
        {
            bulletPower = 1; // Peluru kecil.
        }
        else
        {
            bulletPower = 2; // Peluru sedang.
        }

        SetFire(bulletPower);
        Rescan();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        double bearing = BearingTo(e.X, e.Y);
        if (bearing > -90 && bearing < 90)
            Back(100);
        else
            Forward(100);
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        // Jika terkena peluru, putar 45 derajat ke kanan dan maju. (manuver)
        TurnRight(45);
        Forward(50);
    }
}
