/**
  ******************************************************************************
  * File Name          : main.c
  * Description        : Main program body
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
#include "main.h"
#include "stm32l0xx_hal.h"
#include "adc.h"
#include "lcd.h"
#include "rtc.h"
#include "tim.h"
#include "usart.h"
#include "gpio.h"

/* USER CODE BEGIN Includes */
#include "wfSys.h"
#include "wfEEPROM.h"
#include "CP68_Client.h"
#include "Display.h"
#include "SimSPI.h"
#include "Variables.h"
#include "DP600J.h"
/* USER CODE END Includes */

/* Private variables ---------------------------------------------------------*/

/* USER CODE BEGIN PV */
uint32_t FlashTick;
/* Private variables ---------------------------------------------------------*/

/* USER CODE END PV */

/* Private function prototypes -----------------------------------------------*/
void SystemClock_Config(void);

/* USER CODE BEGIN PFP */
/* Private function prototypes -----------------------------------------------*/

/* USER CODE END PFP */

/* USER CODE BEGIN 0 */
void SysEnterStandby(void)
{
	//__HAL_RCC_APB2_FORCE_RESET();
	//__HAL_RCC_APB1_FORCE_RESET();

	__HAL_RCC_PWR_CLK_ENABLE();

	__HAL_PWR_CLEAR_FLAG(PWR_FLAG_SB);
	__HAL_PWR_CLEAR_FLAG(PWR_FLAG_WU);

	__HAL_RTC_WRITEPROTECTION_DISABLE(&hrtc);//关闭 RTC 写保护

	__HAL_RTC_WAKEUPTIMER_DISABLE_IT(&hrtc,RTC_IT_WUT);
	__HAL_RTC_ALARM_DISABLE_IT(&hrtc,RTC_IT_ALRA|RTC_IT_ALRB);


	__HAL_RTC_ALARM_CLEAR_FLAG(&hrtc,RTC_FLAG_ALRAF|RTC_FLAG_ALRBF);
	__HAL_RTC_TIMESTAMP_CLEAR_FLAG(&hrtc,RTC_FLAG_TSF);
	__HAL_RTC_WAKEUPTIMER_CLEAR_FLAG(&hrtc,RTC_FLAG_WUTF);

	__HAL_RTC_WAKEUPTIMER_ENABLE_IT(&hrtc,RTC_IT_WUT);

	__HAL_RTC_WRITEPROTECTION_ENABLE(&hrtc); //使能 RTC 写保护
	__HAL_PWR_CLEAR_FLAG(PWR_FLAG_WU); //清除 Wake_UP 标志
	HAL_PWR_EnableWakeUpPin(PWR_WAKEUP_PIN1); //设置 WKUP 用于唤醒
	HAL_PWR_EnterSTANDBYMode(); //进入待机模式
}
/* USER CODE END 0 */

int main(void)
{

  /* USER CODE BEGIN 1 */
	uint8_t i;
	uint8_t j;
	uint32_t x32;
	float f;
	RTC_DateTypeDef sDate;
	RTC_TimeTypeDef sTime;
  /* USER CODE END 1 */

  /* MCU Configuration----------------------------------------------------------*/

  /* Reset of all peripherals, Initializes the Flash interface and the Systick. */
  HAL_Init();

  /* USER CODE BEGIN Init */
  VREFINT_CAL = *(__IO uint16_t *)(0x1FF80078);
  /* USER CODE END Init */

  /* Configure the system clock */
  SystemClock_Config();

  /* USER CODE BEGIN SysInit */
  //HAL_PWREx_EnableLowPowerRunMode();
  HAL_ADCEx_EnableVREFINT();
  /* USER CODE END SysInit */

  /* Initialize all configured peripherals */
  MX_GPIO_Init();
  MX_ADC_Init();
  MX_TIM2_Init();
  MX_USART1_UART_Init();
  MX_USART2_UART_Init();
  MX_TIM6_Init();

  /* USER CODE BEGIN 2 */
  //模块初始化
  wfDelay_init(2);
  SimSPI_Init();
  //判断是否为首次开机
//   if(__HAL_PWR_GET_FLAG(PWR_FLAG_WU) || __HAL_PWR_GET_FLAG(PWR_FLAG_SB))
//   {
// 	  __HAL_PWR_CLEAR_FLAG(PWR_FLAG_SB);
// 	  __HAL_PWR_CLEAR_FLAG(PWR_FLAG_WU);  
//   }
//   else
//   {
// 	  __HAL_PWR_CLEAR_FLAG(PWR_FLAG_SB);
// 	  __HAL_PWR_CLEAR_FLAG(PWR_FLAG_WU);
 	  MX_RTC_Init();
//  	  for(i=0;i<3;i++)
//  	  {
//  		  for(j=0;j<8;j++)
//  			HAL_LCD_Write(&hlcd,j,0,0xffffffff);
//  		  HAL_LCD_UpdateDisplayRequest(&hlcd);
//  		  wfDelay_ms(1000);
//  		  HAL_LCD_Clear(&hlcd);
//  		  wfDelay_ms(1000);
//  	  }	  
//   }   
  
  //HAL_HalfDuplex_Init(&CP68_USART_Handle);
  //微调波特率
  huart2.Instance->BRR=0x16;
  huart1.Instance->BRR=0x16;
  HAL_HalfDuplex_EnableReceiver(&CP68_USART_Handle);
  HAL_UART_Receive_IT(&CP68_USART_Handle,aRxBuffer_CP68_Client,1);
  //初始化变量  	
  wfEEPROM_ReadBytes(TCalc_Addr,(uint8_t*)ROMCalcParams.All,32);
  CP68_Client_Init();
  CP68_Client_TxBuf.TxStruct.DeviceType=0x20;//热量表（记热表）    
  for(i=0;i<8;i++)
  {
	  CP68_Client_TxBuf.TxStruct.DeviceAddr[i]=i;
  }
  WorkMode=WorkMode_F;
  curShowPage=1;
  DisplayPage();
  
  //测试代码
  DevicePower_ON();
  //TEST_ON();  
  /* USER CODE END 2 */

  /* Infinite loop */
  /* USER CODE BEGIN WHILE */
  HAL_TIM_PWM_Start(&htim2,TIM_CHANNEL_1);
  j=0;
  while (1)
  {
 	  //wfDelay_ms(1000);
	  //HAL_RTC_GetTime(&hrtc,&Time1,RTC_FORMAT_BCD);
 	 //HAL_UART_Transmit(&CP68_USART_Handle,"1234",4,50);
  /* USER CODE END WHILE */

  /* USER CODE BEGIN 3 */
	  if(GFlags.Bits.bCalcHeat)
	  {
		  GFlags.Bits.bCalcHeat=0;
		  //Read04();
		  ReadADC();
		  //CalcHeat();
	  }
// 	  if(GetDeltaTick(FlashTick)>500)
// 	  {
// 		  FlashTick=HAL_GetTick();
// 		  HAL_RTC_GetDate(&hrtc,&sDate,RTC_FORMAT_BCD);
// 		  HAL_RTC_GetTime(&hrtc,&sTime,RTC_FORMAT_BCD);
// 		  ShowNum(0,HIGH_NIBBLE(sTime.Hours));
// 		  ShowNum(1,LOW_NIBBLE(sTime.Hours));
// 
// 		  ShowNum(3,HIGH_NIBBLE(sTime.Minutes));
// 		  ShowNum(4,LOW_NIBBLE(sTime.Minutes));
// 
// 		  ShowNum(6,HIGH_NIBBLE(sTime.Seconds));
// 		  ShowNum(7,LOW_NIBBLE(sTime.Seconds));
// 		  HAL_LCD_UpdateDisplayRequest(&hlcd);
// 	  }
	  if(CP68_Client_Flags.Bits.bRx)
	  {
		  CP68_Client_CommandProc();
	  }

// 	  wfDelay_ms(1000);
// 	  ReadADC();
	  /*wfEEPROM_ReadBytes(0,(uint8_t*)u8TempList,10);
	  LL_GPIO_SetOutputPin(MCU_TEST_GPIO_Port,MCU_TEST_Pin);
	  LL_ADC_REG_SetSequencerChannels(ADC1, LL_ADC_CHANNEL_1);
	  LL_ADC_REG_StartConversion(ADC1);
	  while(LL_ADC_REG_IsConversionOngoing(ADC1)==1);
	  ADList[0]=ADC1->DR;

	  LL_ADC_REG_SetSequencerChannels(ADC1, LL_ADC_CHANNEL_5);
	  LL_ADC_REG_StartConversion(ADC1);	  	
	  while(LL_ADC_REG_IsConversionOngoing(ADC1)==1);
	  ADList[1]=ADC1->DR;

	  LL_ADC_REG_SetSequencerChannels(ADC1, LL_ADC_CHANNEL_10);
	  LL_ADC_REG_StartConversion(ADC1);
	  while(LL_ADC_REG_IsConversionOngoing(ADC1)==1);
	  ADList[2]=ADC1->DR;

	  LL_ADC_REG_SetSequencerChannels(ADC1, LL_ADC_CHANNEL_VREFINT);
	  LL_ADC_REG_StartConversion(ADC1);
	  while(LL_ADC_REG_IsConversionOngoing(ADC1)==1);
	  LL_GPIO_ResetOutputPin(MCU_TEST_GPIO_Port,MCU_TEST_Pin);
	  ADList[3]=ADC1->DR;*/
  }
  /* USER CODE END 3 */

}

/** System Clock Configuration
*/
void SystemClock_Config(void)
{

  RCC_OscInitTypeDef RCC_OscInitStruct;
  RCC_ClkInitTypeDef RCC_ClkInitStruct;
  RCC_PeriphCLKInitTypeDef PeriphClkInit;

    /**Configure the main internal regulator output voltage 
    */
  __HAL_PWR_VOLTAGESCALING_CONFIG(PWR_REGULATOR_VOLTAGE_SCALE1);

    /**Configure LSE Drive Capability 
    */
  __HAL_RCC_LSEDRIVE_CONFIG(RCC_LSEDRIVE_LOW);

    /**Initializes the CPU, AHB and APB busses clocks 
    */
  RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSI|RCC_OSCILLATORTYPE_LSE;
  RCC_OscInitStruct.LSEState = RCC_LSE_ON;
  RCC_OscInitStruct.HSIState = RCC_HSI_ON;
  RCC_OscInitStruct.HSICalibrationValue = 16;
  RCC_OscInitStruct.PLL.PLLState = RCC_PLL_ON;
  RCC_OscInitStruct.PLL.PLLSource = RCC_PLLSOURCE_HSI;
  RCC_OscInitStruct.PLL.PLLMUL = RCC_PLLMUL_4;
  RCC_OscInitStruct.PLL.PLLDIV = RCC_PLLDIV_2;
  if (HAL_RCC_OscConfig(&RCC_OscInitStruct) != HAL_OK)
  {
    _Error_Handler(__FILE__, __LINE__);
  }

    /**Initializes the CPU, AHB and APB busses clocks 
    */
  RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK|RCC_CLOCKTYPE_SYSCLK
                              |RCC_CLOCKTYPE_PCLK1|RCC_CLOCKTYPE_PCLK2;
  RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_PLLCLK;
  RCC_ClkInitStruct.AHBCLKDivider = RCC_SYSCLK_DIV1;
  RCC_ClkInitStruct.APB1CLKDivider = RCC_HCLK_DIV1;
  RCC_ClkInitStruct.APB2CLKDivider = RCC_HCLK_DIV1;

  if (HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_1) != HAL_OK)
  {
    _Error_Handler(__FILE__, __LINE__);
  }

  PeriphClkInit.PeriphClockSelection = RCC_PERIPHCLK_USART1|RCC_PERIPHCLK_USART2
                              |RCC_PERIPHCLK_RTC;
  PeriphClkInit.Usart1ClockSelection = RCC_USART1CLKSOURCE_LSE;
  PeriphClkInit.Usart2ClockSelection = RCC_USART2CLKSOURCE_LSE;
  PeriphClkInit.RTCClockSelection = RCC_RTCCLKSOURCE_LSE;
  if (HAL_RCCEx_PeriphCLKConfig(&PeriphClkInit) != HAL_OK)
  {
    _Error_Handler(__FILE__, __LINE__);
  }

    /**Configure the Systick interrupt time 
    */
  HAL_SYSTICK_Config(HAL_RCC_GetHCLKFreq()/1000);

    /**Configure the Systick 
    */
  HAL_SYSTICK_CLKSourceConfig(SYSTICK_CLKSOURCE_HCLK);

  /* SysTick_IRQn interrupt configuration */
  HAL_NVIC_SetPriority(SysTick_IRQn, 0, 0);
}

/* USER CODE BEGIN 4 */

/* USER CODE END 4 */

/**
  * @brief  This function is executed in case of error occurrence.
  * @param  None
  * @retval None
  */
void _Error_Handler(char * file, int line)
{
  /* USER CODE BEGIN Error_Handler_Debug */
  /* User can add his own implementation to report the HAL error return state */
  while(1) 
  {
  }
  /* USER CODE END Error_Handler_Debug */ 
}

#ifdef USE_FULL_ASSERT

/**
   * @brief Reports the name of the source file and the source line number
   * where the assert_param error has occurred.
   * @param file: pointer to the source file name
   * @param line: assert_param error line source number
   * @retval None
   */
void assert_failed(uint8_t* file, uint32_t line)
{
  /* USER CODE BEGIN 6 */
  /* User can add his own implementation to report the file name and line number,
    ex: printf("Wrong parameters value: file %s on line %d\r\n", file, line) */
  /* USER CODE END 6 */

}

#endif

/**
  * @}
  */ 

/**
  * @}
*/ 

/************************ (C) COPYRIGHT STMicroelectronics *****END OF FILE****/
