#ifndef   __DP600J_h__
#define   __DP600J_h__

#include "main.h"
#include "TypeDefine.h"
extern uint8_t DP6000J_Data[11];
extern float_wf Press;
extern float_wf curHeat;
extern float_wf curQ;
uint8_t Read04(void);
uint8_t Read06(void);
void CalcHeat(void);
#endif
