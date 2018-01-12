#ifndef   __Variables_h__
#define   __Variables_h__

#include "FlagDefine.h"
#include "TypeDefine.h"
extern volatile uint8_t u8TempList[100];
extern volatile uint16_t u8TempIndex;
extern volatile uint8_t WorkMode;
extern volatile uint8_t curShowPage;
extern volatile uint8_t curShowPageEx;//为0表示未进入最后一级菜单

extern volatile _ROMCalcParams ROMCalcParams;
extern volatile _GFlags GFlags;
extern const float rho[][2];
extern const float h[][2];

extern float_wf Tin;
extern float_wf Tout;
#endif
