#include <Stepper.h>
#include <Wire.h>
#include <Adafruit_MCP23017.h>

#define BACKLIGHT_POTENTIOMETER 15
#define BACKLIGHT_MIN_DUTY 30
#define BACKLIGHT_MAX_DUTY 128

#define BACKLIGHT_WB 12
#define BACKLIGHT_WS 11
#define BACKLIGHT_RB 10
#define BACKLIGHT_RS 9
#define BACKLIGHT_LCD 8

#define MOTOR_STEPS 720
#define MOTOR_SPEED 30
#define MOTOR_A_1 42
#define MOTOR_B_1 38
#define MOTOR_C_1 34
#define MOTOR_D_1 30

Adafruit_MCP23017 LedController;
Stepper GaugeMotorA(MOTOR_STEPS, MOTOR_A_1, MOTOR_A_1 + 1, MOTOR_A_1 + 2, MOTOR_A_1 + 3);
Stepper GaugeMotorB(MOTOR_STEPS, MOTOR_B_1, MOTOR_B_1 + 1, MOTOR_B_1 + 2, MOTOR_B_1 + 3);
Stepper GaugeMotorC(MOTOR_STEPS, MOTOR_C_1, MOTOR_C_1 + 1, MOTOR_C_1 + 2, MOTOR_C_1 + 3);
Stepper GaugeMotorD(MOTOR_STEPS, MOTOR_D_1, MOTOR_D_1 + 1, MOTOR_D_1 + 2, MOTOR_D_1 + 3);

void setup() {
  Serial.begin(9600);
  //LED CONTROLLER
  LedController.begin();
  for(int i = 0; i < 16; i++) LedController.pinMode(i, OUTPUT);
  updateLed("0000000000000000");
  //BACKLIGHT
  pinMode(BACKLIGHT_WB, OUTPUT);
  pinMode(BACKLIGHT_WS, OUTPUT);
  pinMode(BACKLIGHT_RB, OUTPUT);
  pinMode(BACKLIGHT_RS, OUTPUT);
  pinMode(BACKLIGHT_LCD, OUTPUT);
  updateBacklight("00000");
  //GAUGE
  GaugeMotorA.setSpeed(MOTOR_SPEED);
  GaugeMotorB.setSpeed(MOTOR_SPEED);
  GaugeMotorC.setSpeed(MOTOR_SPEED);
  GaugeMotorD.setSpeed(MOTOR_SPEED);
}

void loop() {
  
}

void updateLed(String state) {
  for(int i = 0; i < 16; i++) LedController.digitalWrite(i, state.substring(i, i+1) == "1" ? HIGH : LOW);
}

void updateBacklight(String state){
  int blPower = map(analogRead(BACKLIGHT_POTENTIOMETER), 0, 1023, BACKLIGHT_MIN_DUTY, BACKLIGHT_MAX_DUTY);
  analogWrite(BACKLIGHT_WB, state.substring(0, 1) == "1" ? blPower : 0);
  analogWrite(BACKLIGHT_WS, state.substring(1, 2) == "1" ? blPower : 0);
  digitalWrite(BACKLIGHT_RB, state.substring(2, 3) == "1" ? HIGH : LOW);
  digitalWrite(BACKLIGHT_RS, state.substring(3, 4) == "1" ? HIGH : LOW);
  digitalWrite(BACKLIGHT_LCD, state.substring(4) == "0" ? HIGH : LOW);
}
