#ifndef   __FlagDefine_h__
#define   __FlagDefine_h__

#include "main.h"
/************************************************************************/
/* 第一个为最低位                                                       */
/************************************************************************/
typedef union   
{
	struct
	{
		unsigned bCalcHeat:1;//是否串口收到数据帧	

	}Bits;
	uint8_t AllFlag;
} _GFlags;        // general flags


typedef union   
{
	struct
	{
		float k1;
		float b1;
		float k2;
		float b2;
		float a1;
		float a2;
		float a3;
		float a4;
	}Params;
	uint8_t All[32];
} _ROMCalcParams;        // general flags

typedef union   
{
	struct
	{
		uint32_t BiaoHao;
		uint32_t ChangDai;
		uint32_t DeviceNum;
		uint16_t DN;
		uint8_t SetupPos; //安装位置
		uint8_t bak;//备用
	}Params;
	uint8_t All[16];
} _ROMDeviceParams;        // general flags

#define TCalc_Addr 0	//16个字节：温度计算系数
#define QCalc_Addr 16	//16个字节：流量计算系数

#define AllHeat_Addr 32	//4个字节：累计热量
#define AllQ_Addr 36	//4个字节：累计流量
#define AllTime_Addr 40	//4个字节：累计工作时间
//44-47,预留4个字节备用
#define DeviceParams_Addr 48	//16个字节：设备参数
#define SetupPos_Addr 62


#endif
