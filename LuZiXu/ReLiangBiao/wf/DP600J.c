
#include "DP600J.h"
#include "SimSPI.h"
#include "Verify.h"
#include "Variables.h"
uint8_t DP6000J_Data[11];
float_wf Press;
float_wf curQ;
float_wf curHeat;
uint8_t Read04( void )
{
	uint8_t i;
	ushort_wf crc; 
	SimSPI_Proc(0x04);
	for(i=0;i<11;i++)
	{
		DP6000J_Data[i]=SimSPI_Proc(0x00);
	}
	if(DP6000J_Data[0]!=0x03)
		return 0;
	crc=GetVerify_CRC16(DP6000J_Data,9);
	if(crc.u8H==DP6000J_Data[9] && crc.u8L==DP6000J_Data[10])
	{
		Press.u8s[0]=DP6000J_Data[1];
		Press.u8s[1]=DP6000J_Data[2];
		Press.u8s[2]=DP6000J_Data[3];
		Press.u8s[3]=DP6000J_Data[4];
		curQ.f=ROMCalcParams.Params.a1*Press.f;
		curQ.f+=ROMCalcParams.Params.a2*Press.f*Press.f;
		curQ.f+=ROMCalcParams.Params.a3*Press.f*Press.f*Press.f;
		curQ.f+=ROMCalcParams.Params.a4*Press.f*Press.f*Press.f*Press.f;
		return 1;
	}
	return 0;
}

uint8_t Read06( void )
{
	uint8_t i;
	ushort_wf crc; 
	SimSPI_Proc(0x06);
	for(i=0;i<11;i++)
	{
		DP6000J_Data[i]=SimSPI_Proc(0x00);
	}
	crc=GetVerify_CRC16(DP6000J_Data,9);
	if(crc.u8H==DP6000J_Data[9] && crc.u8L==DP6000J_Data[10])
		return 1;
	return 0;
}

void CalcHeat( void )
{
	uint8_t T1,T2;
	float rho1,rho2,h1,h2;	
	T1=(uint8_t)Tin.f;
	T2=(uint8_t)Tout.f;
	rho1=rho[T1][0]*Tin.f+rho[T1][1];
	rho2=rho[T2][0]*Tout.f+rho[T2][1];
	h1=h[T1][0]*Tin.f+h[T1][1];
	h2=h[T2][0]*Tout.f+h[T2][1];
	//安装在进口
	curHeat.f=rho1*curQ.f*(h1-h2)*(dt-1);
}
