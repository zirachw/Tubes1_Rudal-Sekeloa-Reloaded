using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/* 
------------------------------------------------------------------
Sweepredict
------------------------------------------------------------------
A bot that moves to the nearest corner of the area with padding and then moves along the walls in a square pattern.
It always rotates the radar and gun to track the enemy, and predicts the enemy's future position to fire.
------------------------------------------------------------------
*/

public class Sweepredict : Bot
{
    // Padding borders
    private const double padding = 60;

    // Maximum allowed shooting distance
    private const double maxShootingDistance = 500;

    // Each side of padded area
    private double left, right, bottom, top;

    static void Main()
    {
        new Sweepredict().Start();
    }

    Sweepredict() : base(BotInfo.FromFile("Sweepredict.json")) { }

    public override void Run()
    {
        // Set colors.
        BodyColor = Color.DarkRed;
        TurretColor = Color.Black;
        RadarColor = Color.Orange;
        BulletColor = Color.Red;
        ScanColor = Color.Gray;

        // Enable adjusting gun and radar independent of body
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        // Define the padded rectangle boundaries.
        left   = padding;
        right  = ArenaWidth - padding;
        bottom = padding;
        top    = ArenaHeight - padding;

        // Move to the nearest corner of the padded area
        GoToNearestCorner();

        Console.WriteLine("Direction: " + Direction);

        // Now, the bot's Direction is assumed to be exactly 0, 90, 180, or 270.
        while (IsRunning)
        {
            // Set radar to continuously rotate
            SetTurnRadarRight(Double.PositiveInfinity);
            
            double dir = Direction;
            
            // Direction ~= 0 (Moving from Left to Right)
            if (Math.Abs(dir) < 1 || Math.Abs(dir - 360) < 1)  
            {
                Forward(right - X);
                TurnRight(90);
            }

            // Direction ~= 90 (Moving from Bottom to Top)
            else if (Math.Abs(dir - 90) < 1)  
            {
                Forward(top - Y);
                TurnRight(90);
            }

            // Direction ~= 180 (Moving from Right to Left)
            else if (Math.Abs(dir - 180) < 1)  
            {
                Forward(X - left);
                TurnRight(90);
            }

            // Direction ~= 270 (Moving from Top to Bottom)
            else if (Math.Abs(dir - 270) < 1)  
            {
                Forward(Y - bottom);
                TurnRight(90);
            }

            
            // If we somehow get off track, reset to 0
            else
            {
                TurnToCardinalDirection(0);
            }
        }
    }

    // Method to move to the nearest corner of the padded area
    private void GoToNearestCorner()
    {
        double dBottomLeft = Math.Sqrt(Math.Pow(X - left, 2) + Math.Pow(Y - bottom, 2));
        double dBottomRight = Math.Sqrt(Math.Pow(X - right, 2) + Math.Pow(Y - bottom, 2));
        double dTopLeft = Math.Sqrt(Math.Pow(X - left, 2) + Math.Pow(Y - top, 2));
        double dTopRight = Math.Sqrt(Math.Pow(X - right, 2) + Math.Pow(Y - top, 2));

        // Determine which corner is closest
        double minDist = dBottomLeft;
        string nearestCorner = "bottomLeft";

        if (dBottomRight < minDist) 
        { 
            minDist = dBottomRight; 
            nearestCorner = "bottomRight"; 
        }

        if (dTopLeft < minDist) 
        { 
            minDist = dTopLeft; 
            nearestCorner = "topLeft"; 
        }

        if (dTopRight < minDist) 
        { 
            minDist = dTopRight; 
            nearestCorner = "topRight"; 
        }

        // Navigate to the chosen corner and set appropriate initial direction
        if (nearestCorner == "bottomLeft")
        {
            // Turn towards bottom left corner and facing east
            double angle = Math.Atan2(bottom - Y, left - X) * 180 / Math.PI;
            TurnToCardinalDirection(angle);
            Forward(minDist);
            TurnToCardinalDirection(90);
        }
        else if (nearestCorner == "bottomRight")
        {
            // Turn towards bottom right corner and facing north
            double angle = Math.Atan2(bottom - Y, right - X) * 180 / Math.PI;
            TurnToCardinalDirection(angle);
            Forward(minDist);
            TurnToCardinalDirection(180);
        }
        else if (nearestCorner == "topRight")
        {
            // Turn towards top right corner and facing west
            double angle = Math.Atan2(top - Y, right - X) * 180 / Math.PI;
            TurnToCardinalDirection(angle);
            Forward(minDist);
            TurnToCardinalDirection(270); // Face west
        }
        else // (nearestCorner == "topLeft")
        {
            // Turn towards top left corner and facing south
            double angle = Math.Atan2(top - Y, left - X) * 180 / Math.PI;
            TurnToCardinalDirection(angle);
            Forward(minDist);
            TurnToCardinalDirection(0);
        }
    }
    
    // Helper method to turn to an exact cardinal direction
    private void TurnToCardinalDirection(double targetDirection)
    {
        double turnAmount = NormalizeRelativeAngle(targetDirection - Direction);
        TurnLeft(turnAmount);
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Calculate distance to the scanned bot
        double distance = DistanceTo(e.X, e.Y);
        if (distance > maxShootingDistance)
        {
            // Still track with the gun, but don't fire
            double angleToEnemy = Math.Atan2(e.Y - Y, e.X - X) * 180 / Math.PI;
            double gunTurn = NormalizeRelativeAngle(angleToEnemy - GunDirection);
            SetTurnGunLeft(gunTurn);
            return;
        }
        
        // Determine fire power based on distance
        double firePower = 1.0;                        

        if       (distance <= 50) firePower = 3.0;      // Close range: maximum power
        else if (distance <= 150) firePower = 2.5;      // Medium-close: high power
        else if (distance <= 300) firePower = 2.0;      // Medium range: medium power
        else if (distance <= 450) firePower = 1.5;      // Medium-far: moderate power
        
        // Calculate bullet speed and time to hit
        double bulletSpeed = 20 - 3 * firePower;
        double timeToHit = distance / bulletSpeed;
        
        // Predict enemy's future position
        double enemyHeadingRadians = e.Direction * Math.PI / 180; 
        double predictedX = e.X + Math.Cos(enemyHeadingRadians) * e.Speed * timeToHit;
        double predictedY = e.Y + Math.Sin(enemyHeadingRadians) * e.Speed * timeToHit;
        
        // Calculate angle to the predicted position
        double angleToEnemyPredicted = Math.Atan2(predictedY - Y, predictedX - X) * 180 / Math.PI;
        double gunTurnPredicted = NormalizeRelativeAngle(angleToEnemyPredicted - GunDirection);
        SetTurnGunLeft(gunTurnPredicted);
        
        // Fire when gun is aiming at predicted position (with reasonable tolerance)
        if (Math.Abs(gunTurnPredicted) < 10) SetFire(firePower);
        
        // Ensure continued radar scanning
        Rescan();
    }
}