
#include "Key.h"
#include "gpio.h"
#include "tim.h"
#include "Display.h"
#include "wfSys.h"
#include "lcd.h"
#include "Variables.h"
#include "wfEEPROM.h"


void HAL_TIM_PeriodElapsedCallback(TIM_HandleTypeDef *htim)
{
	GPIO_InitTypeDef GPIO_InitStruct;
	uint8_t x;
	if(htim->Instance==htim6.Instance)
	{
		//长按
		HAL_TIM_Base_Stop_IT(&htim6);		
		GPIO_InitStruct.Pin = Button_Pin;
		GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING;
		GPIO_InitStruct.Pull = GPIO_PULLDOWN;
		HAL_GPIO_Init(Button_GPIO_Port, &GPIO_InitStruct);
		switch(WorkMode)
		{
		case WorkMode_Main:
			if(curShowPage!=0)
				curShowPage=0;
			else
			{
				WorkMode=WorkMode_F;
				curShowPage=0;
			}
			break;
		case WorkMode_F:
			if(curShowPage!=0)
				curShowPage=1;
			else
			{
				WorkMode=WorkMode_l;
				curShowPage=0;
				curShowPageEx=0;
			}
			break;
		case WorkMode_l:
			if(curShowPage!=0)//过了第一页
			{	
				if(curShowPageEx!=0)//已进入子菜单，长按返回主菜单
				{
					curShowPage=0;
					curShowPageEx=0;
				}
				else
				{
					if(curShowPage!=(WorkMode_l_Max-1))
					{
						if(curShowPage==SetupPosPage)
						{
							x=wfEEPROM_ReadByte(SetupPos_Addr);
							if(x==0)//进水口变出水口
							{
								wfEEPROM_WriteByte(SetupPos_Addr,1);
								curShowPageEx=2;
							}
							else
							{
								wfEEPROM_WriteByte(SetupPos_Addr,0);
								curShowPageEx=1;
							}
						}
						else
							curShowPageEx=1;//进入子菜单
					}
					else
						curShowPage=0;//最后一页无子菜单
				}
			}
			else
			{
				WorkMode=WorkMode_E;
				curShowPage=0;
				curShowPageEx=0;
			}
			break;
		case WorkMode_E:
			if(curShowPage!=0)//过了第一页
			{
				if(curShowPageEx!=0)//已进入子菜单，长按返回主菜单
				{
					curShowPage=0;
					curShowPageEx=0;
				}
				else
				{
					if(curShowPage<5)
						curShowPageEx=1;//进入子菜单
					else
						curShowPage=0;//最后一页无子菜单
				}
			}
			else
			{
				WorkMode=WorkMode_Main;
				curShowPage=0;
			}
			break;
		}
		DisplayPage();
	}
}

void HAL_GPIO_EXTI_Callback(uint16_t GPIO_Pin)
{
	GPIO_InitTypeDef GPIO_InitStruct;
	if(GPIO_Pin==Button_Pin)
	{
		if(WF_CHECK_FLAG(htim6.Instance->CR1,TIM_CR1_CEN)!=0)//已经运行了
		{
			//短按
			HAL_TIM_Base_Stop_IT(&htim6);
			GPIO_InitStruct.Pin = Button_Pin;
			GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING;
			GPIO_InitStruct.Pull = GPIO_PULLDOWN;
			HAL_GPIO_Init(Button_GPIO_Port, &GPIO_InitStruct);
			switch(WorkMode)
			{
			case WorkMode_Main:
				curShowPage++;
				if(curShowPage>=WorkMode_Main_Max)
					curShowPage=0;
				break;
			case WorkMode_F:
				curShowPage++;
				if(curShowPage>=WorkMode_F_Max)
					curShowPage=1;
				break;
			case WorkMode_l:
				if(curShowPageEx==0)//未进入子菜单
				{
					curShowPage++;
					if(curShowPage>=WorkMode_l_Max)
						curShowPage=0;
				}
				else
				{
					if(curShowPage==SetupPosPage)//安装位置特殊处理
					{
						if(curShowPageEx==1)
						{
							curShowPageEx=2;
							wfEEPROM_WriteByte(SetupPos_Addr,1);
						}
						else
						{
							curShowPageEx=1;
							wfEEPROM_WriteByte(SetupPos_Addr,0);
						}
					}
					else
					{
						if(curShowPage<19)
						{
							curShowPageEx++;
							if(curShowPageEx>2)
							{
								curShowPageEx=0;
								curShowPage++;
								if(curShowPage>23)
									curShowPage=0;
							}
						}
						else
						{
							curShowPage++;
							if(curShowPage>23)
								curShowPage=0;
						}
					}
				}
				break;
			case WorkMode_E:
				if(curShowPageEx==0)//未进入子菜单
				{
					curShowPage++;
					if(curShowPage>WorkMode_E_Max)
						curShowPage=0;
				}
				else
				{
					curShowPage++;
					if(curShowPage>WorkMode_E_Max)
						curShowPage=0;
					curShowPageEx=0;
				}
				break;
			}
			DisplayPage();
		}
		else
		{
			wfDelay_ms(20);
			if(HAL_GPIO_ReadPin(Button_GPIO_Port,Button_Pin)==GPIO_PIN_SET)
			{
				GPIO_InitStruct.Pin = Button_Pin;
				GPIO_InitStruct.Mode = GPIO_MODE_IT_FALLING;
				GPIO_InitStruct.Pull = GPIO_PULLDOWN;
				HAL_GPIO_Init(Button_GPIO_Port, &GPIO_InitStruct);
				__HAL_TIM_SET_COUNTER(&htim6,0);
				__HAL_TIM_CLEAR_IT(&htim6, TIM_IT_UPDATE);
				HAL_TIM_Base_Start_IT(&htim6);
			}
		}		
	}
}

