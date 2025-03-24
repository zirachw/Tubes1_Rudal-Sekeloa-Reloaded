using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/*
------------------------------------------------------------------
Waves
------------------------------------------------------------------
A bot that moves in a wave pattern along the walls of the arena.
It uses confidence levels by enemy distance and speed to determine when to fire.
------------------------------------------------------------------
*/

public class Waves : Bot
{
    // Size of the wave
    private const double WaveAmplitude = 60; 

    // Controls frequency of waves
    private const double WavePeriod = 40; 

    // Maximum moves to limit continuous waving
    private const int maxMoves = 50;

    // Padding borders
    private const double padding = 30;

    // Maximum allowed shooting distance
    private const double maxShootingDistance = 500;

    // Each side of padded area
    private double left, right, bottom, top;

    // Firepower
    private double firePower;

    static void Main()
    {
        new Waves().Start();
    }

    Waves() : base(BotInfo.FromFile("Waves.json")) { }

    public override void Run()
    {
        // Set colors
        BodyColor = Color.Yellow;
        TurretColor = Color.Black;
        RadarColor = Color.Gray;
        BulletColor = Color.Cyan;
        ScanColor = Color.Green;

        // Define the padded rectangle boundaries
        left = padding;
        right = ArenaWidth - padding;
        bottom = padding;
        top = ArenaHeight - padding;

        // Move to the nearest middle of a side of the padded area
        GoToNearestMiddleOfASide();
        WaitFor(new TurnCompleteCondition(this));

        // Now, the bot's Direction is assumed to be exactly 45, 135, 225, or 315.
        while (IsRunning)
        {   
            // First check if we're out of bounds and handle it efficiently
            if (X < left || X > right || Y < bottom || Y > top)
            {
                HandleOutOfBounds();
            }

            // Keep track of current wave position
            double wavePosition = 0;
            double stepSize = 5;
            int moveCounter = 0;
            
            double distanceX = X - left;
            double distanceY = Y - bottom;
            
            while (distanceX > -3 && 
                   distanceY > -3 && 
                   distanceX < right - left + 3 && 
                   distanceY < top - bottom + 3 && 
                   moveCounter < maxMoves)
            {
                // Calculate the wave angle
                double waveAngle = Math.Sin(wavePosition / WavePeriod) * WaveAmplitude;
                
                // Turn to follow the wave pattern
                SetTurnRight(waveAngle);
                SetForward(stepSize);
                Go();

                distanceX = X - left;
                distanceY = Y - bottom;

                wavePosition += stepSize;
                moveCounter++;
            }
        }
    }

    // Method to move to the nearest middle of a side of the padded area
    private void GoToNearestMiddleOfASide()
    {
        double middleX = (left + right) / 2;
        double middleY = (bottom + top) / 2;

        // Find the distance to the middle point of each side using the built-in DistanceTo method
        double dLeftSide = DistanceTo(left, middleY);
        double dRightSide = DistanceTo(right, middleY);
        double dBottomSide = DistanceTo(middleX, bottom);
        double dTopSide = DistanceTo(middleX, top);
        
        // Determine which side middle is closest
        double minDist = dLeftSide;
        string nearestSide = "left";
        if (dRightSide < minDist) 
        { 
            minDist = dRightSide; 
            nearestSide = "right"; 
        }

        if (dBottomSide < minDist) 
        { 
            minDist = dBottomSide; 
            nearestSide = "bottom"; 
        }

        if (dTopSide < minDist) 
        { 
            minDist = dTopSide; 
            nearestSide = "top"; 
        }
        
        // Check if already at the middle of a side (distance is very close to 0)
        bool alreadyAtMiddle = minDist < 0.001;
        
        if (nearestSide == "left")
        {
            // Turn towards middle of left side and facing northeast
            if (!alreadyAtMiddle)
            {
                double angle = Math.Atan2(middleY - Y, left - X) * 180 / Math.PI;
                TurnToCardinalDirection(angle);
                Forward(minDist);
                WaitFor(new MovementCompleteCondition(this));
            }

            TurnToCardinalDirection(45);
        }
        else if (nearestSide == "right")
        {
            // Turn towards middle of right side and facing southwest
            if (!alreadyAtMiddle)
            {
                double angle = Math.Atan2(middleY - Y, right - X) * 180 / Math.PI;
                TurnToCardinalDirection(angle);
                Forward(minDist);
                WaitFor(new MovementCompleteCondition(this));
            }

            TurnToCardinalDirection(225);
        }
        else if (nearestSide == "bottom")
        {
            // Turn towards middle of bottom side and facing northwest
            if (!alreadyAtMiddle)
            {
                double angle = Math.Atan2(bottom - Y, middleX - X) * 180 / Math.PI;
                TurnToCardinalDirection(angle);
                Forward(minDist);
                WaitFor(new MovementCompleteCondition(this));
            }

            TurnToCardinalDirection(135);
        }
        else // (nearestSide == "top")
        {
            
            // Turn towards middle of top side and facing southeast
            if (!alreadyAtMiddle)
            {
                double angle = Math.Atan2(top - Y, middleX - X) * 180 / Math.PI;
                TurnToCardinalDirection(angle);
                Forward(minDist);
                WaitFor(new MovementCompleteCondition(this));
            }
            TurnToCardinalDirection(315);
        }
    }
    
    // Helper method to turn to an exact cardinal direction
    private void TurnToCardinalDirection(double targetDirection)
    {
        double turnAmount = NormalizeRelativeAngle(targetDirection - Direction);
        TurnLeft(turnAmount);
        WaitFor(new TurnCompleteCondition(this));
    }

    // Handle out of bounds situations efficiently
    private void HandleOutOfBounds()
    {
        // Stop any current movement
        Stop();
        
        double targetDirection = -1;
        
        // Determine which boundary we've crossed
        if      (X < left && Y < bottom) targetDirection = 45;      // Northeast
        else if (X < left && Y > top) targetDirection = 315;        // Southeast
        else if (X > right && Y < bottom) targetDirection = 135;    // Northwest
        else if (X > right && Y > top) targetDirection = 225;       // Southwest
        else if (X < left) targetDirection = 0;                     // East
        else if (X > right) targetDirection = 180;                  // West
        else if (Y < bottom) targetDirection = 90;                  // North
        else if (Y > top) targetDirection = 270;                    // South
        
        if (targetDirection != -1)
        {
            double turnAmount = NormalizeRelativeAngle(targetDirection - Direction);
            
            SetTurnLeft(turnAmount);
            Go();
            WaitFor(new TurnCompleteCondition(this));
            
            double moveDistance = 40;
            SetForward(moveDistance);
            Go();
            WaitFor(new MovementCompleteCondition(this));
        }
    }

    // When we hit a wall, adjust our course to properly recover
    public override void OnHitWall(HitWallEvent e)
    {
        Stop();
        
        bool hitLeftWall = X <= left + 5;
        bool hitRightWall = X >= right - 5;
        bool hitBottomWall = Y <= bottom + 5;
        bool hitTopWall = Y >= top - 5;
        
        double retreatDirection = Direction;
        
        if      (hitLeftWall) retreatDirection = 0;     // Move east
        else if (hitRightWall) retreatDirection = 180;  // Move west
        else if (hitBottomWall) retreatDirection = 90;  // Move north
        else if (hitTopWall) retreatDirection = 270;    // Move south
        
        double turnAmount = NormalizeRelativeAngle(retreatDirection - Direction);
        
        SetTurnLeft(turnAmount);
        Go();
        WaitFor(new TurnCompleteCondition(this));
        
        SetForward(50);
        Go();
        WaitFor(new MovementCompleteCondition(this));
        
        GoToNearestMiddleOfASide();
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Calculate distance to the scanned bot
        double distance = DistanceTo(e.X, e.Y);
        if (distance > maxShootingDistance) return;
        
        // Determine fire power based on distance
        firePower = calculateFirePower(distance);
        
        // Calculate aiming confidence
        double aimConfidence;
        if (distance < 50) 
        {
            // Very high confidence for close targets
            aimConfidence = 0.95; 
        } 
        else 
        {
            aimConfidence = 1.0;
            aimConfidence *= 1.0 - (distance / 1000.0);             // Distance factor
            aimConfidence *= 1.0 - (Math.Min(8, e.Speed) / 16.0);   // Speed factor
        }
        
        // Fire if confident enough
        if (aimConfidence > 0.3)
        {
            // Close combat keeps full power, longer distances may reduce power slightly
            double adjustedFirePower = distance < 50 ? firePower : firePower * Math.Max(0.8, aimConfidence);
            Fire(adjustedFirePower);
        }
        
        // Ensure continued radar scanning
        Rescan();
    }

    // Improved collision handling with other bots
    public override void OnHitBot(HitBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double bearing = BearingTo(e.X, e.Y);
        
        // Being rammed by another bot
        if (e.IsRammed) 
        {
            Stop();
            
            // Turn gun toward enemy and fire with power based on distance
            double gunTurn = NormalizeRelativeAngle(bearing - GunDirection);
            TurnGunLeft(gunTurn);
            
            // Fire with power based on distance
            firePower = calculateFirePower(distance);
            Fire(firePower);
        }

        // Rammed into another bot
        else 
        {
            Stop();
            
            double gunTurn = NormalizeRelativeAngle(bearing - GunDirection);
            TurnGunLeft(gunTurn);
            
            // Fire with power based on distance
            firePower = calculateFirePower(distance);
            Fire(firePower);
        }
    }

    // Determine fire power based on distance
    public double calculateFirePower(double distance)
    {
        firePower = 1.0;                        

        if      (distance <= 50)  firePower = 3.0;      // Close range: maximum power
        else if (distance <= 150) firePower = 2.5;      // Medium-close: high power
        else if (distance <= 300) firePower = 2.0;      // Medium range: medium power
        else if (distance <= 450) firePower = 1.5;      // Medium-far: moderate power

        return firePower;
    }
}

// Condition that is triggered when the turning is complete
public class TurnCompleteCondition : Condition
{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test() => bot.TurnRemaining == 0;
}

// Condition that is triggered when the movement is complete
public class MovementCompleteCondition : Condition
{
    private readonly Bot bot;

    public MovementCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test() => bot.DistanceRemaining == 0;
}