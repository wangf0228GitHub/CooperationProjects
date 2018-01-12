#ifndef __SimSPI_Conf_h__
#define __SimSPI_Conf_h__

#include "gpio.h"
#include "wfSys.h"

#define SimSPI_SPI3
#define SimSPI_Delay_Ex
#define SimSPI_Delay() wfDelay_ms(1);


#define SIMSPI_SCL_Low() HAL_GPIO_WritePin(YALI_SCLK_GPIO_Port,YALI_SCLK_Pin,GPIO_PIN_RESET)
#define SIMSPI_SCL_High() HAL_GPIO_WritePin(YALI_SCLK_GPIO_Port,YALI_SCLK_Pin,GPIO_PIN_SET)

#define SIMSPI_SDO_Low() HAL_GPIO_WritePin(YALI_MOSI_GPIO_Port,YALI_MOSI_Pin,GPIO_PIN_RESET)
#define SIMSPI_SDO_High() HAL_GPIO_WritePin(YALI_MOSI_GPIO_Port,YALI_MOSI_Pin,GPIO_PIN_SET)

#define SIMSPI_SDI_Read() HAL_GPIO_ReadPin(YALI_MISO_GPIO_Port,YALI_MISO_Pin)
#endif


