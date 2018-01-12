#ifndef   __Display_h__
#define   __Display_h__

#include "main.h"

#define WorkMode_Main_Max 12
#define WorkMode_F_Max 6
#define WorkMode_l_Max 23
#define WorkMode_E_Max 6

#define SetupPosPage 21

#define Seg_s1	0//累积
#define Seg_s2  1//热量
#define Seg_s3  2//冷量
#define Seg_s4  3//流量
#define Seg_s5  4//功率
#define Seg_s6  5//进水
#define Seg_s7  6//回水
#define Seg_s8  7//温差 

#define Seg_h1  8//检定
#define Seg_h2  9//报警
#define Seg_h3  10//DP3
#define Seg_h4  11//DP4
#define Seg_h5  12//DP5
#define Seg_h6  13//DP6
#define Seg_h7  14//DP7

#define Seg_t1  15//P1
#define Seg_t2  16//P2
#define Seg_t3  17//P3
#define Seg_t4  18//P4
#define Seg_t5  19//地址
#define Seg_t6  20//日期
#define Seg_t7  21//时间

#define Seg_B	22
#define Seg_B1	23
#define Seg_B2	24
#define Seg_B3	25

#define Seg_a1  26//℃
#define Seg_k1  27//K
#define Seg_k	28//k
#define Seg_W	29//W
#define Seg_a6  30//.h
#define Seg_a7  31//MW.h
#define Seg_GJ  32//GJ
#define Seg_MJ  33//MJ
#define Seg_a8  34//m3
#define Seg__h  35//_h



void ShowNum(uint8_t n,uint8_t x);
void DisplayPage(void);
#endif
