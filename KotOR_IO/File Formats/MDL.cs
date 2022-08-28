using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO
{
    public class MDL : KFile
    {
        #region Constants
         const int kNodeFlagHasHeader = 0x0001;
         const int kNodeFlagHasLight = 0x0002;
         const int kNodeFlagHasEmitter = 0x0004;
         const int kNodeFlagHasReference = 0x0010;
         const int kNodeFlagHasMesh = 0x0020;
         const int kNodeFlagHasSkin = 0x0040;
         const int kNodeFlagHasAnim = 0x0080;
         const int kNodeFlagHasDangly = 0x0100;
         const int kNodeFlagHasAABB = 0x0200;
         const int kNodeFlagHasSaber = 0x0800;

         const uint kControllerTypePosition = 8;
         const uint kControllerTypeOrientation = 20;
         const uint kControllerTypeScale = 36;
         const uint kControllerTypeColor = 76;
         const uint kControllerTypeRadius = 88;
         const uint kControllerTypeShadowRadius = 96;
         const uint kControllerTypeVerticalDisplacement = 100;
         const uint kControllerTypeMultiplier = 140;
         const uint kControllerTypeAlphaEnd = 80;
         const uint kControllerTypeAlphaStart = 84;
         const uint kControllerTypeBirthRate = 88;
         const uint kControllerTypeBounce_Co = 92;
         const uint kControllerTypeColorEnd = 96;
         const uint kControllerTypeColorStart = 108;
         const uint kControllerTypeCombineTime = 120;
         const uint kControllerTypeDrag = 124;
         const uint kControllerTypeFPS = 128;
         const uint kControllerTypeFrameEnd = 132;
         const uint kControllerTypeFrameStart = 136;
         const uint kControllerTypeGrav = 140;
         const uint kControllerTypeLifeExp = 144;
         const uint kControllerTypeMass = 148;
         const uint kControllerTypeP2P_Bezier2 = 152;
         const uint kControllerTypeP2P_Bezier3 = 156;
         const uint kControllerTypeParticleRot = 160;
         const uint kControllerTypeRandVel = 164;
         const uint kControllerTypeSizeStart = 168;
         const uint kControllerTypeSizeEnd = 172;
         const uint kControllerTypeSizeStart_Y = 176;
         const uint kControllerTypeSizeEnd_Y = 180;
         const uint kControllerTypeSpread = 184;
         const uint kControllerTypeThreshold = 188;
         const uint kControllerTypeVelocity = 192;
         const uint kControllerTypeXSize = 196;
         const uint kControllerTypeYSize = 200;
         const uint kControllerTypeBlurLength = 204;
         const uint kControllerTypeLightningDelay = 208;
         const uint kControllerTypeLightningRadius = 212;
         const uint kControllerTypeLightningScale = 216;
         const uint kControllerTypeDetonate = 228;
         const uint kControllerTypeAlphaMid = 464;
         const uint kControllerTypeColorMid = 468;
         const uint kControllerTypePercentStart = 480;
         const uint kControllerTypePercentMid = 481;
         const uint kControllerTypePercentEnd = 482;
         const uint kControllerTypeSizeMid = 484;
         const uint kControllerTypeSizeMid_Y = 488;
         const uint kControllerTypeSelfIllumColor = 100;
         const uint kControllerTypeAlpha = 128;
        #endregion Constants

        internal override void Write(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}
