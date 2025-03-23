using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using Robocode.TankRoyale.BotApi.Util;

// ------------------------------------------------------------------
// WideScannerFollower with Greedy Targeting
// ------------------------------------------------------------------
// This bot now selects its target greedily: when multiple enemies are
// scanned, it tracks the one with the lowest energy.
// ------------------------------------------------------------------
public class WideScannerFollower : Bot
{
    private double trackingDistance = 200; // Desired distance from enemy
    private double tolerance = 20;         // Tolerance range for distance adjustment
    private ScannedBotEvent lastScannedBot = null;

    private long currentTick = 0;
    private double lastEnemyX = 0;
    private double lastEnemyY = 0;
    private long lastScanTick = 0;
    private bool hasPreviousScan = false;

    // The main method starts our bot.
    static void Main()
    {
        new WideScannerFollower().Start();
    }

    WideScannerFollower() : base(BotInfo.FromFile("GeorgeDroyd.json")) { }

    public override void Run()
    {
        // Set colors.
        BodyColor   = Color.FromArgb(0x64, 0x64, 0xFF);  // blue
        TurretColor = Color.FromArgb(0x32, 0x32, 0xC8);  // dark blue
        RadarColor  = Color.FromArgb(0x00, 0x64, 0x64);  // dark cyan
        BulletColor = Color.FromArgb(0xFF, 0xFF, 0x64);  // yellow
        ScanColor   = Color.FromArgb(0x64, 0xC8, 0xFF);  // light blue

        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        while (IsRunning)
        {
            currentTick++; 
            SetTurnRadarRight(360);
            SetTurnRadarLeft(360);
            if (lastScannedBot == null)
            {
                SetTurnRadarRight(360);
                SetTurnRadarLeft(360);
            }
            else
            {
                TrackTarget();
            }
            
            Go();
        }
    }

    private void TrackTarget()
    {
        if (lastScannedBot == null) return;

        double enemyX = lastScannedBot.X;
        double enemyY = lastScannedBot.Y;
        double angleToEnemy = Math.Atan2(enemyY - Y, enemyX - X) * 180 / Math.PI;
        double distanceToEnemy = Math.Sqrt((enemyX - X) * (enemyX - X) + (enemyY - Y) * (enemyY - Y));

        double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
        SetTurnGunLeft(gunTurn);

        double distanceError = distanceToEnemy - trackingDistance;
        if (Math.Abs(distanceError) > tolerance)
        {
            if (distanceError > 0)
            {
                SetForward(Math.Min(distanceError, 100));
            }
            else
            {
                SetBack(Math.Min(-distanceError, 100));
            }
        }
        else
        {
            double strafeAngle = NormalizeAngle(angleToEnemy + 90);
            double turnToStrafe = NormalizeRelativeAngle(strafeAngle - Direction);
            SetTurnRight(turnToStrafe);
            SetForward(70);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {

        if (lastScannedBot == null || e.ScannedBotId == lastScannedBot.ScannedBotId || e.Energy < lastScannedBot.Energy)
        {
            lastScannedBot = e;
        }

        double angleToEnemy = Math.Atan2(e.Y - Y, e.X - X) * 180 / Math.PI;
        double radarTurn = NormalizeRelativeAngle(angleToEnemy - RadarDirection);
        radarTurn *= 2;
        SetTurnRadarLeft(radarTurn);

        double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
        SetTurnGunLeft(gunTurn);

        Console.WriteLine($"Scanned bot: {e.ScannedBotId} at ({e.X}, {e.Y})");
        double distanceToEnemy = Math.Sqrt((e.X - X) * (e.X - X) + (e.Y - Y) * (e.Y - Y));
        if (Math.Abs(gunTurn) < 10 && distanceToEnemy < 300 && GunHeat == 0)
        {
            Fire(Math.Min(3.0, 400 / distanceToEnemy));
        }

        lastEnemyX = e.X;
        lastEnemyY = e.Y;
        lastScanTick = currentTick;
        hasPreviousScan = true;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        SetTurnRight(45);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        if(currentTick % 2 == 0)
        {
            SetTurnRight(90);
        }
        else
        {
            SetTurnLeft(90);
        }
    }

    private double NormalizeAngle(double angle)
    {
        while (angle >= 360) angle -= 360;
        while (angle < 0) angle += 360;
        return angle;
    }

    private double NormalizeRelativeAngle(double angle)
    {
        while (angle > 180) angle -= 360;
        while (angle <= -180) angle += 360;
        return angle;
    }
}
