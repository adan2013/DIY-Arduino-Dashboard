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

#define MOTOR_LATENCY 4
#define MOTOR_MAX_STEP 40
#define MOTOR_BIG_STEPS 600
#define MOTOR_BIG_SPEED 60
#define MOTOR_SMALL_STEPS 210
#define MOTOR_SMALL_SPEED 180

#define MOTOR_A_1 38
#define MOTOR_B_1 34
#define MOTOR_C_1 30
#define MOTOR_D_1 26

Adafruit_MCP23017 ledController;
Stepper gaugeMotorA(MOTOR_BIG_STEPS, MOTOR_A_1, MOTOR_A_1 + 1, MOTOR_A_1 + 2, MOTOR_A_1 + 3);
Stepper gaugeMotorB(MOTOR_SMALL_STEPS, MOTOR_B_1, MOTOR_B_1 + 1, MOTOR_B_1 + 2, MOTOR_B_1 + 3);
Stepper gaugeMotorC(MOTOR_SMALL_STEPS, MOTOR_C_1, MOTOR_C_1 + 1, MOTOR_C_1 + 2, MOTOR_C_1 + 3);
Stepper gaugeMotorD(MOTOR_BIG_STEPS, MOTOR_D_1, MOTOR_D_1 + 1, MOTOR_D_1 + 2, MOTOR_D_1 + 3);
int gaugeCurrentStep[4];
int gaugeTargetStep[4];

bool left = true;
unsigned long lastSwitch = 0;

void splitData(String input, int count, String *output) {
  int cursorPosition = 0;
  int idx = 0;
  for(int i = 0; i < input.length(); i++) {
    if(input.substring(i, i + 1) == "@") {
      output[idx] = input.substring(cursorPosition, i);
      cursorPosition = i + 1;
      idx++;
      if(idx == count - 1) break;
    }
  }
  output[count - 1] = input.substring(cursorPosition, input.length());
}

void updateLeds(String state) {
  for(int i = 0; i < 16; i++) ledController.digitalWrite(i, state.substring(i, i+1) == "1" ? HIGH : LOW);
}

void updateBacklights(String state){
  int blPower = map(analogRead(BACKLIGHT_POTENTIOMETER), 0, 1023, BACKLIGHT_MIN_DUTY, BACKLIGHT_MAX_DUTY);
  analogWrite(BACKLIGHT_WB, state.substring(0, 1) == "1" ? blPower : 0);
  analogWrite(BACKLIGHT_WS, state.substring(1, 2) == "1" ? blPower : 0);
  digitalWrite(BACKLIGHT_RB, state.substring(2, 3) == "1" ? HIGH : LOW);
  digitalWrite(BACKLIGHT_RS, state.substring(3, 4) == "1" ? HIGH : LOW);
  digitalWrite(BACKLIGHT_LCD, state.substring(4) == "0" ? HIGH : LOW);
}

void updateGauges(String state) {
  String values[4];
  splitData(state, 4, values);
  for(int i = 0; i < 4; i++) {
    gaugeTargetStep[i] = values[i].toFloat() * (i == 0 || i == 3 ? MOTOR_BIG_STEPS : MOTOR_SMALL_STEPS) * 0.01;
  }
}

void moveMotors() {
  for(int i = 0; i < 4; i++) {
    int diff = gaugeCurrentStep[i] - gaugeTargetStep[i];
    if(abs(diff) >= MOTOR_LATENCY) {
      diff = min(abs(diff), MOTOR_MAX_STEP) * (diff < 0 ? -1 : 1);
      Serial.print("CURRENT: ");
      Serial.print(gaugeCurrentStep[0]);
      Serial.print(" TARGET: ");
      Serial.print(gaugeTargetStep[0]);
      Serial.print(" DIFF: ");
      Serial.print(diff);
      Serial.print("\n");
      switch(i) {
        case 0: gaugeMotorA.step(diff); break;
        case 1: gaugeMotorB.step(diff); break;
        case 2: gaugeMotorC.step(diff); break;
        case 3: gaugeMotorD.step(diff); break;
      }
      gaugeCurrentStep[i] = gaugeCurrentStep[i] - diff;
    }
  }
}

void resetLeds() {
  updateLeds("0000000000000000");
}

void resetBacklights() {
  updateBacklights("00000");
}

void resetGauges() {
  updateGauges("0@0@0@0");
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
  gaugeMotorA.setSpeed(MOTOR_BIG_SPEED);
  gaugeMotorB.setSpeed(MOTOR_SMALL_SPEED);
  gaugeMotorC.setSpeed(MOTOR_SMALL_SPEED);
  gaugeMotorD.setSpeed(MOTOR_BIG_SPEED);
  resetGauges();


  updateGauges("100@0@0@0");
  lastSwitch = millis();
}

void loop() {
//  updateGauges("255@0@0@0");
//  String setup = "";
//  for(int ledAnim = 0; ledAnim < 16; ledAnim++) {
//    setup = "";
//    for(int i = 0; i < 16; i++) {
//      if(i == ledAnim || i == (15 - ledAnim)) {
//        setup += "1";
//      }else{
//        setup += "0";
//      }
//    }
//    updateLeds(setup);
//    delay(200);
//  }
//  resetLeds();
//  updateGauges("0@0@0@0");
//  delay(1000);
//  updateBacklights("10000");
//  delay(1000);
//  updateBacklights("11000");
//  delay(1000);
//  updateBacklights("11100");
//  delay(1000);
//  updateBacklights("11110");
//  delay(1000);
//  updateBacklights("11111");
//  delay(1000);
//  resetBacklights();
//  delay(1000);
  if(millis() - lastSwitch > 300) {
    lastSwitch = millis();
    left = !left;
  }
  if(left){
    updateLeds("1111111100000000");
  }else{
    updateLeds("0000000011111111");
  }
  moveMotors();
  if(gaugeCurrentStep[0] > 580) updateGauges("0@0@0@0");
}
