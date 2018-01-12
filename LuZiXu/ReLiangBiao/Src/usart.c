/**
  ******************************************************************************
  * File Name          : USART.c
  * Description        : This file provides code for the configuration
  *                      of the USART instances.
  ******************************************************************************
  ** This notice applies to any and all portions of this file
  * that are not between comment pairs USER CODE BEGIN and
  * USER CODE END. Other portions of this file, whether 
  * inserted by the user or by software development tools
  * are owned by their respective copyright owners.
  *
  * COPYRIGHT(c) 2017 STMicroelectronics
  *
  * Redistribution and use in source and binary forms, with or without modification,
  * are permitted provided that the following conditions are met:
  *   1. Redistributions of source code must retain the above copyright notice,
  *      this list of conditions and the following disclaimer.
  *   2. Redistributions in binary form must reproduce the above copyright notice,
  *      this list of conditions and the following disclaimer in the documentation
  *      and/or other materials provided with the distribution.
  *   3. Neither the name of STMicroelectronics nor the names of its contributors
  *      may be used to endorse or promote products derived from this software
  *      without specific prior written permission.
  *
  * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
  * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
  * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
  * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
  * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
  * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
  * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
  * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
  * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
  * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  *
  ******************************************************************************
  */

/* Includes ------------------------------------------------------------------*/
#include "usart.h"

#include "gpio.h"

/* USER CODE BEGIN 0 */
#include "CP68_Client.h"
#include "adc.h"
#include "wfSys.h"
#include "wfEEPROM.h"
#include "Variables.h"
#include "TypeDefine.h"
#include "DP600J.h"
#include "rtc.h"
uint8_t aRxBuffer_CP68_Client[1];
void HAL_UART_RxCpltCallback(UART_HandleTypeDef* UartHandle)
{
	if(UartHandle->Instance==CP68_USART_Handle.Instance)
	{
		CP68_Client_ProcRx(aRxBuffer_CP68_Client[0]);
		HAL_UART_Receive_IT(&CP68_USART_Handle,aRxBuffer_CP68_Client,1);	
	}
}
void HAL_UART_ErrorCallback(UART_HandleTypeDef *huart)
{
	HAL_UART_Receive_IT(&CP68_USART_Handle,aRxBuffer_CP68_Client,1);	
}
void CP68_Client_CommandProc(void)
{
	uint8_t i;
	float_wf f;
	RTC_TimeTypeDef sTime;
	RTC_DateTypeDef sDate;
	CP68_Client_CheckRxBuf();
	if(!CP68_Client_Flags.Bits.bValid)
		return;
	HAL_HalfDuplex_EnableTransmitter(&CP68_USART_Handle);
	switch(CP68_Client_RxBuf.RxStruct.ControlCode)
	{
	case CP68_UserCommand_ReadAD:		
		ReadADC();
		CP68_Client_TxBuf.TxStruct.DataBuf[0]=LOW_BYTE(Temp1);
		CP68_Client_TxBuf.TxStruct.DataBuf[1]=HIGH_BYTE(Temp1);
		CP68_Client_TxBuf.TxStruct.DataBuf[2]=LOW_BYTE(Temp2);
		CP68_Client_TxBuf.TxStruct.DataBuf[3]=HIGH_BYTE(Temp2);
		CP68_Client_TxBuf.TxStruct.DataBuf[4]=LOW_BYTE(VDDIn);
		CP68_Client_TxBuf.TxStruct.DataBuf[5]=HIGH_BYTE(VDDIn);
		CP68_Client_TxBuf.TxStruct.DataBuf[6]=LOW_BYTE(VREFINT_CAL);
		CP68_Client_TxBuf.TxStruct.DataBuf[7]=HIGH_BYTE(VREFINT_CAL);
		for(i=0;i<4;i++)
		{
			CP68_Client_TxBuf.TxStruct.DataBuf[8+i]=Tin.u8s[i];
			CP68_Client_TxBuf.TxStruct.DataBuf[12+i]=Tout.u8s[i];
		}
		CP68_Client_SendSelf(CP68_Client_RxBuf.RxStruct.ControlCode,16);
		break;
	case CP68_UserCommand_WriteTCalc:
		for(i=0;i<16;i++)
			ROMCalcParams.All[i]=CP68_Client_RxBuf.RxStruct.DataBuf[i];
		CP68_Client_SendSelf(CP68_Client_RxBuf.RxStruct.ControlCode,0);
		wfEEPROM_WriteBytes(TCalc_Addr,(uint8_t*)ROMCalcParams.All,16);
		break;
	case CP68_UserCommand_ReadPress:
		if(Read04()==0)
		{
			f.f=-1;
			for(i=0;i<4;i++)
			{
				CP68_Client_TxBuf.TxStruct.DataBuf[i]=f.u8s[i];
				CP68_Client_TxBuf.TxStruct.DataBuf[i+4]=f.u8s[i];
			}
			CP68_Client_SendSelf(CP68_Client_RxBuf.RxStruct.ControlCode,8);
		}
		else
		{
			for(i=0;i<4;i++)
			{
				CP68_Client_TxBuf.TxStruct.DataBuf[i]=Press.u8s[i];
				f.f=ROMCalcParams.Params.a1*Press.f;
				f.f+=ROMCalcParams.Params.a2*Press.f*Press.f;
				f.f+=ROMCalcParams.Params.a3*Press.f*Press.f*Press.f;
				f.f+=ROMCalcParams.Params.a4*Press.f*Press.f*Press.f*Press.f;
				CP68_Client_TxBuf.TxStruct.DataBuf[i+4]=f.u8s[i];
			}
			CP68_Client_SendSelf(CP68_Client_RxBuf.RxStruct.ControlCode,8);
		}		
		break;
	case CP68_UserCommand_WriteQCalc:
		for(i=0;i<16;i++)
			ROMCalcParams.All[i+16]=CP68_Client_RxBuf.RxStruct.DataBuf[i];
		CP68_Client_SendSelf(CP68_Client_RxBuf.RxStruct.ControlCode,0);
		wfEEPROM_WriteBytes(QCalc_Addr,(uint8_t*)(&(ROMCalcParams.All[16])),16);
		break;
	case CP68_UserCommand_WriteDeviceParams:
		sDate.Year = CP68_Client_RxBuf.RxStruct.DataBuf[0];
		sDate.Month = CP68_Client_RxBuf.RxStruct.DataBuf[1];
		sDate.Date = CP68_Client_RxBuf.RxStruct.DataBuf[2];		
		sDate.WeekDay = CP68_Client_RxBuf.RxStruct.DataBuf[3];
		HAL_RTC_SetDate(&hrtc, &sDate, RTC_FORMAT_BIN);
		sTime.Hours = CP68_Client_RxBuf.RxStruct.DataBuf[4];
		sTime.Minutes = CP68_Client_RxBuf.RxStruct.DataBuf[5];
		sTime.Seconds = CP68_Client_RxBuf.RxStruct.DataBuf[6];
		sTime.DayLightSaving = RTC_DAYLIGHTSAVING_NONE;
		sTime.StoreOperation = RTC_STOREOPERATION_RESET;
		HAL_RTC_SetTime(&hrtc, &sTime, RTC_FORMAT_BIN);
		CP68_Client_SendSelf(CP68_Client_RxBuf.RxStruct.ControlCode,0);
		break;
	}
	CP68_Client_EndProcCommand();
	HAL_HalfDuplex_EnableReceiver(&CP68_USART_Handle);	
	HAL_UART_Receive_IT(&CP68_USART_Handle,aRxBuffer_CP68_Client,1);	
}
/* USER CODE END 0 */

UART_HandleTypeDef huart1;
UART_HandleTypeDef huart2;

/* USART1 init function */

void MX_USART1_UART_Init(void)
{

  huart1.Instance = USART1;
  huart1.Init.BaudRate = 2400;
  huart1.Init.WordLength = UART_WORDLENGTH_9B;
  huart1.Init.StopBits = UART_STOPBITS_1;
  huart1.Init.Parity = UART_PARITY_EVEN;
  huart1.Init.Mode = UART_MODE_TX_RX;
  huart1.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart1.Init.OverSampling = UART_OVERSAMPLING_8;
  huart1.Init.OneBitSampling = UART_ONE_BIT_SAMPLE_DISABLE;
  huart1.AdvancedInit.AdvFeatureInit = UART_ADVFEATURE_NO_INIT;
  if (HAL_UART_Init(&huart1) != HAL_OK)
  {
    _Error_Handler(__FILE__, __LINE__);
  }

}
/* USART2 init function */

void MX_USART2_UART_Init(void)
{

  huart2.Instance = USART2;
  huart2.Init.BaudRate = 2400;
  huart2.Init.WordLength = UART_WORDLENGTH_9B;
  huart2.Init.StopBits = UART_STOPBITS_1;
  huart2.Init.Parity = UART_PARITY_EVEN;
  huart2.Init.Mode = UART_MODE_TX_RX;
  huart2.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart2.Init.OverSampling = UART_OVERSAMPLING_8;
  huart2.Init.OneBitSampling = UART_ONE_BIT_SAMPLE_DISABLE;
  huart2.AdvancedInit.AdvFeatureInit = UART_ADVFEATURE_NO_INIT;
  if (HAL_UART_Init(&huart2) != HAL_OK)
  {
    _Error_Handler(__FILE__, __LINE__);
  }

}

void HAL_UART_MspInit(UART_HandleTypeDef* uartHandle)
{

  GPIO_InitTypeDef GPIO_InitStruct;
  if(uartHandle->Instance==USART1)
  {
  /* USER CODE BEGIN USART1_MspInit 0 */

  /* USER CODE END USART1_MspInit 0 */
    /* USART1 clock enable */
    __HAL_RCC_USART1_CLK_ENABLE();
  
    /**USART1 GPIO Configuration    
    PB6     ------> USART1_TX
    PB7     ------> USART1_RX 
    */
    GPIO_InitStruct.Pin = GPIO_PIN_6|GPIO_PIN_7;
    GPIO_InitStruct.Mode = GPIO_MODE_AF_PP;
    GPIO_InitStruct.Pull = GPIO_PULLUP;
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
    GPIO_InitStruct.Alternate = GPIO_AF0_USART1;
    HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

    /* USART1 interrupt Init */
    HAL_NVIC_SetPriority(USART1_IRQn, 1, 0);
    HAL_NVIC_EnableIRQ(USART1_IRQn);
  /* USER CODE BEGIN USART1_MspInit 1 */

  /* USER CODE END USART1_MspInit 1 */
  }
  else if(uartHandle->Instance==USART2)
  {
  /* USER CODE BEGIN USART2_MspInit 0 */

  /* USER CODE END USART2_MspInit 0 */
    /* USART2 clock enable */
    __HAL_RCC_USART2_CLK_ENABLE();
  
    /**USART2 GPIO Configuration    
    PA2     ------> USART2_TX
    PA3     ------> USART2_RX 
    */
    GPIO_InitStruct.Pin = GPIO_PIN_2|GPIO_PIN_3;
    GPIO_InitStruct.Mode = GPIO_MODE_AF_PP;
    GPIO_InitStruct.Pull = GPIO_PULLUP;
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
    GPIO_InitStruct.Alternate = GPIO_AF4_USART2;
    HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

    /* USART2 interrupt Init */
    HAL_NVIC_SetPriority(USART2_IRQn, 1, 0);
    HAL_NVIC_EnableIRQ(USART2_IRQn);
  /* USER CODE BEGIN USART2_MspInit 1 */

  /* USER CODE END USART2_MspInit 1 */
  }
}

void HAL_UART_MspDeInit(UART_HandleTypeDef* uartHandle)
{

  if(uartHandle->Instance==USART1)
  {
  /* USER CODE BEGIN USART1_MspDeInit 0 */

  /* USER CODE END USART1_MspDeInit 0 */
    /* Peripheral clock disable */
    __HAL_RCC_USART1_CLK_DISABLE();
  
    /**USART1 GPIO Configuration    
    PB6     ------> USART1_TX
    PB7     ------> USART1_RX 
    */
    HAL_GPIO_DeInit(GPIOB, GPIO_PIN_6|GPIO_PIN_7);

    /* USART1 interrupt Deinit */
    HAL_NVIC_DisableIRQ(USART1_IRQn);
  /* USER CODE BEGIN USART1_MspDeInit 1 */

  /* USER CODE END USART1_MspDeInit 1 */
  }
  else if(uartHandle->Instance==USART2)
  {
  /* USER CODE BEGIN USART2_MspDeInit 0 */

  /* USER CODE END USART2_MspDeInit 0 */
    /* Peripheral clock disable */
    __HAL_RCC_USART2_CLK_DISABLE();
  
    /**USART2 GPIO Configuration    
    PA2     ------> USART2_TX
    PA3     ------> USART2_RX 
    */
    HAL_GPIO_DeInit(GPIOA, GPIO_PIN_2|GPIO_PIN_3);

    /* USART2 interrupt Deinit */
    HAL_NVIC_DisableIRQ(USART2_IRQn);
  /* USER CODE BEGIN USART2_MspDeInit 1 */

  /* USER CODE END USART2_MspDeInit 1 */
  }
} 

/* USER CODE BEGIN 1 */

/* USER CODE END 1 */

/**
  * @}
  */

/**
  * @}
  */

/************************ (C) COPYRIGHT STMicroelectronics *****END OF FILE****/
