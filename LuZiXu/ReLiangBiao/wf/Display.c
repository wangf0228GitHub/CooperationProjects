
#include "Display.h"
#include "lcd.h"
#include "wfSys.h"
#include "wfEEPROM.h"
#include "Variables.h"
#include "Display_Conf.h"
#include "DP600J.h"

void ShowNum(uint8_t n,uint8_t x)
{
	uint8_t num=Numbers[x];
	uint8_t i;
	for(i=0;i<7;i++)
	{
		if((num&0x01)!=0)//显示
		{
			HAL_LCD_Write(&hlcd,NumCom[n][i],~(1<<NumSeg[n][i]),1<<NumSeg[n][i]);
		}
		else
		{
			HAL_LCD_Write(&hlcd,NumCom[n][i],~(1<<NumSeg[n][i]),0);
		}
		num=num>>1;
	}
}
void DisplayPage_Main( void )
{
	uint8_t show[10],n,i,offset;
	uint32_t data;
	switch(curShowPage)
	{
	case 0://累积热量
		SHOWSEG(Seg_s1);
		SHOWSEG(Seg_s2);
		SHOWSEG(Seg_k);
		SHOWSEG(Seg_W);
		SHOWSEG(Seg_a6);
		SHOWSEG(Seg_h6);
		data=wfEEPROM_ReadWord(AllHeat_Addr);
 		n=sprintf(show,"%03d",data);
 		if(n>8)
 			offset=0;
 		else
 			offset=8-n;
 		for(i=0;i<n;i++)
 		{
 			ShowNum(offset+i,show[i]-'0');
 		}
		break;
	case 1://累积流量
		SHOWSEG(Seg_s1);
		SHOWSEG(Seg_s4);
		SHOWSEG(Seg_a8);
		SHOWSEG(Seg_h6);
		data=wfEEPROM_ReadWord(AllQ_Addr);
		n=sprintf(show,"%03d",data);
		if(n>8)
			offset=0;
		else
			offset=8-n;
		for(i=0;i<n;i++)
		{
			ShowNum(offset+i,show[i]-'0');
		}
		break;
	case 2://进水
		SHOWSEG(Seg_s6);
		SHOWSEG(Seg_a1);		
		break;
	case 3://回水
		SHOWSEG(Seg_s7);
		SHOWSEG(Seg_a1);		
		break;
	case 4://温差
		SHOWSEG(Seg_s8);
		SHOWSEG(Seg_k1);		
		break;
	case 5://流量
		SHOWSEG(Seg_s4);
		SHOWSEG(Seg_a8);
		SHOWSEG(Seg__h);
		break;
	case 6://日期
		SHOWSEG(Seg_t6);
		break;
	case 7://时间
		SHOWSEG(Seg_t7);
		break;
	case 8://累计时间
		SHOWSEG(Seg_s1);
		SHOWSEG(Seg_t7);
		SHOWSEG(Seg_a6);
		break;
	case 9://编号
		SHOWSEG(Seg_t5);
		break;
	case 10://DNxx
		SHOWSEG(Seg_h1);
		break;
	case 11://U-125
		SHOWSEG(Seg_h2);
		break;
	}
}
void DisplayPage_F( void )
{
	uint8_t show[10],n,i,offset;
	uint32_t data;
	
	switch(curShowPage)
	{
	case 0://[ F ]
		ShowNum(2,0x0c);
		ShowNum(4,0x0f);
		ShowNum(6,17);
		break;
	case 1://累积热量
		SHOWSEG(Seg_h1);//检定
		SHOWSEG(Seg_s1);
		SHOWSEG(Seg_s2);
		SHOWSEG(Seg_k);
		SHOWSEG(Seg_W);
		SHOWSEG(Seg_a6);
		//SHOWSEG(Seg_a8);
		SHOWSEG(Seg_h6);
		data=(uint32_t)(curHeat.f*100);
		n=sprintf(show,"%03d",data);
		if(n>8)
			offset=0;
		else
			offset=8-n;
		for(i=0;i<n;i++)
		{
			ShowNum(offset+i,show[i]-'0');
		}
		break;
	case 2://流量
		SHOWSEG(Seg_h1);//检定
		SHOWSEG(Seg_s4);
		SHOWSEG(Seg_a8);
		SHOWSEG(Seg__h);
		SHOWSEG(Seg_h6);
		data=(uint32_t)(curQ.f*100);
		n=sprintf(show,"%03d",data);
		if(n>8)
			offset=0;
		else
			offset=8-n;
		for(i=0;i<n;i++)
		{
			ShowNum(offset+i,show[i]-'0');
		}
		break;
	case 3://进水
		SHOWSEG(Seg_h1);//检定
		SHOWSEG(Seg_s6);
		SHOWSEG(Seg_a1);	
		SHOWSEG(Seg_h6);
		data=(uint32_t)(Tin.f*100);
		n=sprintf(show,"%03d",data);
		if(n>8)
			offset=0;
		else
			offset=8-n;
		for(i=0;i<n;i++)
		{
			ShowNum(offset+i,show[i]-'0');
		}
		break;
	case 4://回水
		SHOWSEG(Seg_h1);//检定
		SHOWSEG(Seg_s7);
		SHOWSEG(Seg_a1);
		SHOWSEG(Seg_h6);
		data=(uint32_t)(Tout.f*100);
		n=sprintf(show,"%03d",data);
		if(n>8)
			offset=0;
		else
			offset=8-n;
		for(i=0;i<n;i++)
		{
			ShowNum(offset+i,show[i]-'0');
		}
		break;
	case 5://温差
		SHOWSEG(Seg_h1);//检定
		SHOWSEG(Seg_s8);
		SHOWSEG(Seg_k1);
		SHOWSEG(Seg_h6);
		data=(uint32_t)((Tin.f-Tout.f)*100);
		n=sprintf(show,"%03d",data);
		if(n>8)
			offset=0;
		else
			offset=8-n;
		for(i=0;i<n;i++)
		{
			ShowNum(offset+i,show[i]-'0');
		}
		break;	
	}
}
void DisplayPage_l( void )
{
	uint8_t show[10],n,i,offset;
	uint32_t data;
	if(curShowPage==0)
	{
		ShowNum(2,0x0c);
		ShowNum(4,0x01);
		ShowNum(6,17);
	}
	else if(curShowPage<=18)//18个月数据存储
	{
		if(curShowPageEx==0)
		{
			//d_xx.xx.xx
			ShowNum(0,0x0d);
			ShowNum(1,18);
			SHOWSEG(Seg_h4);
			SHOWSEG(Seg_h6);
		}
		else if(curShowPageEx==1)
		{
			//累计热量
			SHOWSEG(Seg_s1);
			SHOWSEG(Seg_s2);
		}
		else if(curShowPageEx==2)
		{
			//累计流量
			SHOWSEG(Seg_s1);
			SHOWSEG(Seg_s4);
		}
	}
	else if(curShowPage==19)//P_Adr
	{
		if(curShowPageEx==0)
		{
			//P_Adr
			ShowNum(0,19);
			ShowNum(1,18);
			ShowNum(2,0x0A);
			ShowNum(3,0x0d);
			ShowNum(4,20);
		}
		else if(curShowPageEx==1)
		{
			//地址
			SHOWSEG(Seg_t5);
		}
	}
	else if(curShowPage==20)//S_Adr
	{
		if(curShowPageEx==0)
		{
			//S_Adr
			ShowNum(0,5);
			ShowNum(1,18);
			ShowNum(2,0x0A);
			ShowNum(3,0x0d);
			ShowNum(4,20);
		}
		else if(curShowPageEx==1)
		{
			//地址
			SHOWSEG(Seg_t5);
		}
	}
	else if(curShowPage==SetupPosPage)//安装位置
	{
		if(curShowPageEx==0)
		{
			n=wfEEPROM_ReadByte(SetupPos_Addr);
			if(n==0)
			{
				//in
				ShowNum(0,1);
				ShowNum(1,21);
			}
			else
			{
				ShowNum(0,0);
				ShowNum(1,22);
				ShowNum(2,23);
			}			
		}
		else if(curShowPageEx==1)
		{
			ShowNum(0,1);
			ShowNum(1,21);			
		}
		else 
		{
			ShowNum(0,0);
			ShowNum(1,22);
			ShowNum(2,23);
		}
	}
	else if(curShowPage==22)//U-125
	{
		ShowNum(0,22);
		ShowNum(1,24);
		ShowNum(2,1);
		ShowNum(3,2);
		ShowNum(4,5);
	}
}
void DisplayPage_E( void )
{
	uint8_t show[10],n,i,offset;
	uint32_t data;
	switch(curShowPage)
	{
	case 0://[ E ]
		ShowNum(2,0x0c);
		ShowNum(4,0x0E);
		ShowNum(6,17);
		break;
	case 1://Err_1
		if(curShowPageEx==0)
		{
			ShowNum(0,0x0e);
			ShowNum(1,20);
			ShowNum(2,20);
			ShowNum(3,18);
			ShowNum(4,1);
		}
		else
		{
			SHOWSEG(Seg_t6);
			SHOWSEG(Seg_h4);
			SHOWSEG(Seg_h6);
			ShowNum(0,0x0d);
			ShowNum(1,18);
		}
		break;
	case 2://Err_2
		if(curShowPageEx==0)
		{
			ShowNum(0,0x0e);
			ShowNum(1,20);
			ShowNum(2,20);
			ShowNum(3,18);
			ShowNum(4,2);
		}
		else
		{
			SHOWSEG(Seg_t6);
			SHOWSEG(Seg_h4);
			SHOWSEG(Seg_h6);
			ShowNum(0,0x0d);
			ShowNum(1,18);
		}
		break;
	case 3://Err_3
		if(curShowPageEx==0)
		{
			ShowNum(0,0x0e);
			ShowNum(1,20);
			ShowNum(2,20);
			ShowNum(3,18);
			ShowNum(4,3);
		}
		else
		{
			SHOWSEG(Seg_t6);
			SHOWSEG(Seg_h4);
			SHOWSEG(Seg_h6);
			ShowNum(0,0x0d);
			ShowNum(1,18);
		}
		break;
	case 4://Err_4
		if(curShowPageEx==0)
		{
			ShowNum(0,0x0e);
			ShowNum(1,20);
			ShowNum(2,20);
			ShowNum(3,18);
			ShowNum(4,4);
		}
		else
		{
			SHOWSEG(Seg_t6);
			SHOWSEG(Seg_h4);
			SHOWSEG(Seg_h6);
			ShowNum(0,0x0d);
			ShowNum(1,18);
		}
		break;
	case 5://累积热量
		SHOWSEG(Seg_s1);
		SHOWSEG(Seg_s2);
		SHOWSEG(Seg_k);
		SHOWSEG(Seg_W);
		SHOWSEG(Seg_a6);
		SHOWSEG(Seg_h6);
		data=wfEEPROM_ReadWord(AllHeat_Addr);
		n=sprintf(show,"%03d",data);
		if(n>8)
			offset=0;
		else
			offset=8-n;
		for(i=0;i<n;i++)
		{
			ShowNum(offset+i,show[i]-'0');
		}
		break;
	case 6://累积流量
		SHOWSEG(Seg_s1);
		SHOWSEG(Seg_s4);
		SHOWSEG(Seg_a8);
		SHOWSEG(Seg_h6);
		data=wfEEPROM_ReadWord(AllQ_Addr);
		n=sprintf(show,"%03d",data);
		if(n>8)
			offset=0;
		else
			offset=8-n;
		for(i=0;i<n;i++)
		{
			ShowNum(offset+i,show[i]-'0');
		}
		break;
	}
}
void DisplayPage( void )
{
	HAL_LCD_Clear(&hlcd);
	switch (WorkMode)
	{
	case WorkMode_Main:
		DisplayPage_Main();
		break;
	case WorkMode_F:
		DisplayPage_F();
		break;
	case WorkMode_l:
		DisplayPage_l();
		break;
	case WorkMode_E:
		DisplayPage_E();
		break;
	}
	HAL_LCD_UpdateDisplayRequest(&hlcd);
}
