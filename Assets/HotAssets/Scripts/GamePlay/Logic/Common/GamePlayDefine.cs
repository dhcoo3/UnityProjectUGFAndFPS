namespace HotAssets.Scripts.GamePlay.Logic.Common
{
    public class GamePlayDefine
    {
        public class CreateId
        {
            public const int Unit = 100000;
            public const int Effect = 200000;
        }
        
        public class LoadPriority
        {
            public const int Scene = 1;
            public const int Role = 2;
            public const int UI = 3;
            public const int Bullet = 4;
            public const int Effect = 5;
            public const int Aoe = 6;
        }
        
        public enum RoleType
        {
            Null = 0,
            Hero = 1,
            Monster = 2
        }

        public enum EAimDirection
        {
            Right      = 0,
            RightUp    = 1,
            Up         = 2,
            LeftUp     = 3,
            Left       = 4,
            LeftDown   = 5,
            Down       = 6,
            RightDown  = 7,
        }

        public static fix AimDirectionToDegree(EAimDirection dir)
        {
            switch (dir)
            {
                case EAimDirection.Right:     return new fix(0);
                case EAimDirection.RightUp:   return new fix(45);
                case EAimDirection.Up:        return new fix(90);
                case EAimDirection.LeftUp:    return new fix(135);
                case EAimDirection.Left:      return new fix(180);
                case EAimDirection.LeftDown:  return new fix(225);
                case EAimDirection.Down:      return new fix(270);
                case EAimDirection.RightDown: return new fix(315);
                default:                      return new fix(0);
            }
        }
        
        public enum EntityGroup
        {
            RoleEntity,
            BulletEntity,
            MonsterEntity,
            PlatformEntity
        }
        
        public enum SoundGroup
        {
            Music,
            Sound,
        }
    }
}