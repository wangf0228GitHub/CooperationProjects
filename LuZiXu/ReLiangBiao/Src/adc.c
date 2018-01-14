/**
  ******************************************************************************
  * File Name          : ADC.c
  * Description        : This file provides code for the configuration
  *                      of the ADC instances.
  ******************************************************************************
  ** This notice applies to any and all portions of this file
  * that are not between comment pairs USER CODE BEGIN and
  * USER CODE END. Other portions of this file, whether 
  * inserted by the user or by software development tools
  * are owned by their respective copyright owners.
  *
  * COPYRIGHT(c) 2018 STMicroelectronics
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
#include "adc.h"

#include "gpio.h"

/* USER CODE BEGIN 0 */
#include "Variables.h"
#include "ADS1112.h"
#include "wfSys.h"
uint16_t Temp1;
uint16_t Temp2;
uint16_t VDDIn;
volatile uint16_t VREFINT_CAL;
void ReadADC(void)
{
	uint8_t i;
	float v;
	Temp1=0;
	for(i=0;i<8;i++)
	{
		ADS1112_Status.INP=0b00;//温度
		ADS1112_Status.SC=1;//
		ADS1112_Status.ST_DRDY=1;
		ADS1112_Status.PGA=0b11;
		ADS1112_Status.DR=0b11;
		if(ADS1112_Write()==0)//出错
		{
			return;
		}
		wfDelay_ms(1);
		while(1)
		{
			if(ADS1112_Read()==0)//出错
			{						
				return;
			}
			if(ADS1112_Status.ST_DRDY==0)//有新数据了
				break;
		}	
		Temp1+=ADS1112_Result.u16>>3;
	}
	Temp2=0;
	for(i=0;i<8;i++)
	{
 		ADS1112_Status.INP=0b01;//温度
 		ADS1112_Status.SC=1;//
 		ADS1112_Status.ST_DRDY=1;
 		ADS1112_Status.PGA=0b11;
 		ADS1112_Status.DR=0b11;
 		if(ADS1112_Write()==0)//出错
 		{
 			return;
 		}
 		wfDelay_ms(5);
 		while(1)
 		{
 			if(ADS1112_Read()==0)//出错
 			{						
 				return;
 			}
 			if(ADS1112_Status.ST_DRDY==0)//有新数据了
 				break;
 		}
		Temp2+=ADS1112_Result.u16>>3;
	}
// 	v=3.0 * VREFINT_CAL;
// 	v=v/VDDIn;
// 	Tin.f=Temp1*v;
// 	Tin.f=Tin.f/4096;
	Tin.f=Temp1*ROMCalcParams.Params.k1+ROMCalcParams.Params.b1;
// 	Tout.f=Temp2*v;
// 	Tout.f=Tout.f/4096;
	Tout.f=Temp2*ROMCalcParams.Params.k2+ROMCalcParams.Params.b2;


// 	float v;
// 	HAL_StatusTypeDef st;
// 	ADC_ChannelConfTypeDef sConfig;
// 	hadc.Instance->CHSELR=0;
// 	sConfig.Channel = ADC_CHANNEL_10;
// 	sConfig.Rank = ADC_RANK_CHANNEL_NUMBER;
// 	if (HAL_ADC_ConfigChannel(&hadc, &sConfig) != HAL_OK)
// 	{
// 		_Error_Handler(__FILE__, __LINE__);
// 	}
// 	HAL_ADC_Start(&hadc);
// 	HAL_ADC_PollForConversion(&hadc,100);
// 
// 	Temp1=HAL_ADC_GetValue(&hadc);
// 
// 	hadc.Instance->CHSELR=0;
// 	sConfig.Channel = ADC_CHANNEL_1;
// 	sConfig.Rank = ADC_RANK_CHANNEL_NUMBER;
// 	if (HAL_ADC_ConfigChannel(&hadc, &sConfig) != HAL_OK)
// 	{
// 		_Error_Handler(__FILE__, __LINE__);
// 	}
// 	HAL_ADC_Start(&hadc);
// 	HAL_ADC_PollForConversion(&hadc,100);
// 
// 	Temp2=HAL_ADC_GetValue(&hadc);
// 
// 
// 	hadc.Instance->CHSELR=0;
// 	sConfig.Channel = ADC_CHANNEL_VREFINT;
// 	sConfig.Rank = ADC_RANK_CHANNEL_NUMBER;
// 	if (HAL_ADC_ConfigChannel(&hadc, &sConfig) != HAL_OK)
// 	{
// 		_Error_Handler(__FILE__, __LINE__);
// 	}
// 	HAL_ADC_Start(&hadc);
// 	HAL_ADC_PollForConversion(&hadc,100);
// 
// 	VDDIn=HAL_ADC_GetValue(&hadc);
// 
// 	v=3.0 * VREFINT_CAL;
// 	v=v/VDDIn;
// 	Tin.f=Temp1*v;
// 	Tin.f=Tin.f/4096;
// 	Tin.f=Tin.f*ROMCalcParams.Params.k1+ROMCalcParams.Params.b1;
// 	Tout.f=Temp2*v;
// 	Tout.f=Tout.f/4096;
// 	Tout.f=Tout.f*ROMCalcParams.Params.k2+ROMCalcParams.Params.b2;

}
/* USER CODE END 0 */

ADC_HandleTypeDef hadc;

/* ADC init function */
void MX_ADC_Init(void)
{
  ADC_ChannelConfTypeDef sConfig;

    /**Configure the global features of the ADC (Clock, Resolution, Data Alignment and number of conversion) 
    */
  hadc.Instance = ADC1;
  hadc.Init.OversamplingMode = ENABLE;
  hadc.Init.Oversample.Ratio = ADC_OVERSAMPLING_RATIO_4;
  hadc.Init.Oversample.RightBitShift = ADC_RIGHTBITSHIFT_2;
  hadc.Init.Oversample.TriggeredMode = ADC_TRIGGEREDMODE_SINGLE_TRIGGER;
  hadc.Init.ClockPrescaler = ADC_CLOCK_ASYNC_DIV64;
  hadc.Init.Resolution = ADC_RESOLUTION_12B;
  hadc.Init.SamplingTime = ADC_SAMPLETIME_3CYCLES_5;
  hadc.Init.ScanConvMode = ADC_SCAN_DIRECTION_FORWARD;
  hadc.Init.DataAlign = ADC_DATAALIGN_RIGHT;
  hadc.Init.ContinuousConvMode = DISABLE;
  hadc.Init.DiscontinuousConvMode = DISABLE;
  hadc.Init.ExternalTrigConvEdge = ADC_EXTERNALTRIGCONVEDGE_NONE;
  hadc.Init.ExternalTrigConv = ADC_SOFTWARE_START;
  hadc.Init.DMAContinuousRequests = DISABLE;
  hadc.Init.EOCSelection = ADC_EOC_SINGLE_CONV;
  hadc.Init.Overrun = ADC_OVR_DATA_OVERWRITTEN;
  hadc.Init.LowPowerAutoWait = DISABLE;
  hadc.Init.LowPowerFrequencyMode = DISABLE;
  hadc.Init.LowPowerAutoPowerOff = DISABLE;
  if (HAL_ADC_Init(&hadc) != HAL_OK)
  {
    _Error_Handler(__FILE__, __LINE__);
  }

    /**Configure for the selected ADC regular channel to be converted. 
    */
  sConfig.Channel = ADC_CHANNEL_VREFINT;
  sConfig.Rank = ADC_RANK_CHANNEL_NUMBER;
  if (HAL_ADC_ConfigChannel(&hadc, &sConfig) != HAL_OK)
  {
    _Error_Handler(__FILE__, __LINE__);
  }

}

void HAL_ADC_MspInit(ADC_HandleTypeDef* adcHandle)
{

  GPIO_InitTypeDef GPIO_InitStruct;
  if(adcHandle->Instance==ADC1)
  {
  /* USER CODE BEGIN ADC1_MspInit 0 */

  /* USER CODE END ADC1_MspInit 0 */
    /* ADC1 clock enable */
    __HAL_RCC_ADC1_CLK_ENABLE();
  
    /**ADC GPIO Configuration    
    PA5     ------> ADC_IN5 
    */
    GPIO_InitStruct.Pin = BT_AD_Pin;
    GPIO_InitStruct.Mode = GPIO_MODE_ANALOG;
    GPIO_InitStruct.Pull = GPIO_NOPULL;
    HAL_GPIO_Init(BT_AD_GPIO_Port, &GPIO_InitStruct);

  /* USER CODE BEGIN ADC1_MspInit 1 */
	ADS1112_Init();
  /* USER CODE END ADC1_MspInit 1 */
  }
}

void HAL_ADC_MspDeInit(ADC_HandleTypeDef* adcHandle)
{

  if(adcHandle->Instance==ADC1)
  {
  /* USER CODE BEGIN ADC1_MspDeInit 0 */

  /* USER CODE END ADC1_MspDeInit 0 */
    /* Peripheral clock disable */
    __HAL_RCC_ADC1_CLK_DISABLE();
  
    /**ADC GPIO Configuration    
    PA5     ------> ADC_IN5 
    */
    HAL_GPIO_DeInit(BT_AD_GPIO_Port, BT_AD_Pin);

  /* USER CODE BEGIN ADC1_MspDeInit 1 */

  /* USER CODE END ADC1_MspDeInit 1 */
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
