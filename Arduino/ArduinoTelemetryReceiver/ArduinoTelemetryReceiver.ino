#include <Stepper.h>
#include <Wire.h>
#include <Adafruit_MCP23017.h>

#define BACKLIGHT_POTENTIOMETER A8
#define BACKLIGHT_MIN_DUTY 10
#define BACKLIGHT_MAX_DUTY 128

#define BACKLIGHT_WB 12
#define BACKLIGHT_WS 11
#define BACKLIGHT_RB 10
#define BACKLIGHT_RS 9
#define BACKLIGHT_LCD 8

#define MOTOR_STEPS 720
#define MOTOR_SPEED 30
#define MOTOR_A_1 38
#define MOTOR_B_1 34
#define MOTOR_C_1 30
#define MOTOR_D_1 26

Adafruit_MCP23017 ledController;
Stepper gaugeMotorA(MOTOR_STEPS, MOTOR_A_1, MOTOR_A_1 + 1, MOTOR_A_1 + 2, MOTOR_A_1 + 3);
Stepper gaugeMotorB(MOTOR_STEPS, MOTOR_B_1, MOTOR_B_1 + 1, MOTOR_B_1 + 2, MOTOR_B_1 + 3);
Stepper gaugeMotorC(MOTOR_STEPS, MOTOR_C_1, MOTOR_C_1 + 1, MOTOR_C_1 + 2, MOTOR_C_1 + 3);
Stepper gaugeMotorD(MOTOR_STEPS, MOTOR_D_1, MOTOR_D_1 + 1, MOTOR_D_1 + 2, MOTOR_D_1 + 3);

void updateLed(String state) {
  for(int i = 0; i < 16; i++) ledController.digitalWrite(i, state.substring(i, i+1) == "1" ? HIGH : LOW);
}

void updateBacklight(String state){
  int blPower = map(analogRead(BACKLIGHT_POTENTIOMETER), 0, 1023, BACKLIGHT_MIN_DUTY, BACKLIGHT_MAX_DUTY);
  analogWrite(BACKLIGHT_WB, state.substring(0, 1) == "1" ? blPower : 0);
  analogWrite(BACKLIGHT_WS, state.substring(1, 2) == "1" ? blPower : 0);
  digitalWrite(BACKLIGHT_RB, state.substring(2, 3) == "1" ? HIGH : LOW);
  digitalWrite(BACKLIGHT_RS, state.substring(3, 4) == "1" ? HIGH : LOW);
  digitalWrite(BACKLIGHT_LCD, state.substring(4) == "0" ? HIGH : LOW);
}

void resetLeds() {
  updateLed("0000000000000000");
}

void resetBacklights() {
  updateBacklight("00000");
}

void setup() {
  Serial.begin(9600);
  //LED CONTROLLER
  ledController.begin();
  for(int i = 0; i < 16; i++) ledController.pinMode(i, OUTPUT);
  resetLeds();
  //BACKLIGHT
  pinMode(BACKLIGHT_WB, OUTPUT);
  pinMode(BACKLIGHT_WS, OUTPUT);
  pinMode(BACKLIGHT_RB, OUTPUT);
  pinMode(BACKLIGHT_RS, OUTPUT);
  pinMode(BACKLIGHT_LCD, OUTPUT);
  resetBacklights();
  //GAUGE
  gaugeMotorA.setSpeed(MOTOR_SPEED);
  gaugeMotorB.setSpeed(MOTOR_SPEED);
  gaugeMotorC.setSpeed(MOTOR_SPEED);
  gaugeMotorD.setSpeed(MOTOR_SPEED);
}

void loop() {
  String setup = "";
  for(int ledAnim = 0; ledAnim < 16; ledAnim++) {
    setup = "";
    for(int i = 0; i < 16; i++) {
      if(i == ledAnim || i == (15 - ledAnim)) {
        setup += "1";
      }else{
        setup += "0";
      }
    }
    updateLed(setup);
    delay(200);
  }
  resetLeds();
  delay(1000);
  updateBacklight("10000");
  delay(1000);
  updateBacklight("11000");
  delay(1000);
  updateBacklight("11100");
  delay(1000);
  updateBacklight("11110");
  delay(1000);
  updateBacklight("11111");
  delay(1000);
  resetBacklights();
  delay(1000);
}
