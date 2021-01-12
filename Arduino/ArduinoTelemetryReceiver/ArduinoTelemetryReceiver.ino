#include <Stepper.h>
#include <Wire.h>
#include <Adafruit_MCP23017.h>
#include <Adafruit_GFX.h>
#include <Adafruit_ILI9341.h>

#define BACKLIGHT_POTENTIOMETER A8
#define BACKLIGHT_MIN_DUTY 10
#define BACKLIGHT_MAX_DUTY 128
#define BLINK_LENGTH 600

#define BACKLIGHT_WB 12
#define BACKLIGHT_WS 11
#define BACKLIGHT_RB 10
#define BACKLIGHT_RS 9
#define BACKLIGHT_LCD 8

#define MOTOR_BIG_STEPS 600
#define MOTOR_BIG_SPEED 60
#define MOTOR_SMALL_STEPS 170
#define MOTOR_SMALL_SPEED 60

#define MOTOR_A_1 38
#define MOTOR_B_1 34
#define MOTOR_C_1 30
#define MOTOR_D_1 26

#define TFT_CS 48
#define TFT_DC 49

String serialBuffer = "";
boolean serialDataIsReady = false;

long lastBlinkStateSwitch = 0;
bool ledBlinkState = false;
String ledState = "0000000000000000";
String backlightState = "00000";
int currentBacklightDuty = 0;

Adafruit_ILI9341 tft = Adafruit_ILI9341(TFT_CS, TFT_DC);
int w;
int h;
String regA[5];
String regB[5];
String regC[5];
bool regChanges[15];
int screenId = 0;

Adafruit_MCP23017 ledController;
Stepper gaugeMotorA(MOTOR_BIG_STEPS, MOTOR_A_1, MOTOR_A_1 + 1, MOTOR_A_1 + 2, MOTOR_A_1 + 3);
Stepper gaugeMotorB(MOTOR_SMALL_STEPS, MOTOR_B_1, MOTOR_B_1 + 1, MOTOR_B_1 + 2, MOTOR_B_1 + 3);
Stepper gaugeMotorC(MOTOR_SMALL_STEPS, MOTOR_C_1, MOTOR_C_1 + 1, MOTOR_C_1 + 2, MOTOR_C_1 + 3);
Stepper gaugeMotorD(MOTOR_BIG_STEPS, MOTOR_D_1, MOTOR_D_1 + 1, MOTOR_D_1 + 2, MOTOR_D_1 + 3);
int gaugeCurrentStep[4];
int gaugeTargetStep[4];

void splitData(String input, int count, String *output) {
  int cursorPosition = 0;
  int idx = 0;
  for(int i = 0; i < input.length(); i++) {
    if(input.substring(i, i + 1) == "_") {
      output[idx] = input.substring(cursorPosition, i);
      cursorPosition = i + 1;
      idx++;
      if(idx == count - 1) break;
    }
  }
  output[count - 1] = input.substring(cursorPosition, input.length());
}

void updateLeds(String state) {
  if(state.length() == 16) {
    ledState = state;
    for(int i = 0; i < 16; i++) {
      String val = state.substring(i, i+1);
      if(val == "2") {
        ledController.digitalWrite(i, ledBlinkState ? HIGH : LOW);
      }else{
        ledController.digitalWrite(i, val == "1" ? HIGH : LOW);
      }
    }
  }
}

void updateBlinkingLeds() {
  if(millis() - lastBlinkStateSwitch >= BLINK_LENGTH) {
    lastBlinkStateSwitch = millis();
    ledBlinkState = !ledBlinkState;
    for(int i = 0; i < 16; i++) {
      if(ledState.substring(i, i+1) == "2") ledController.digitalWrite(i, ledBlinkState ? HIGH : LOW);
    }
  }
}

void updateBacklights(String state){
  if(state.length() == 5) {
    backlightState = state;
    int blPower = map(analogRead(BACKLIGHT_POTENTIOMETER), 0, 1023, BACKLIGHT_MIN_DUTY, BACKLIGHT_MAX_DUTY);
    analogWrite(BACKLIGHT_WB, state.substring(0, 1) == "1" ? blPower : 0);
    analogWrite(BACKLIGHT_WS, state.substring(1, 2) == "1" ? blPower : 0);
    digitalWrite(BACKLIGHT_RB, state.substring(2, 3) == "1" ? HIGH : LOW);
    digitalWrite(BACKLIGHT_RS, state.substring(3, 4) == "1" ? HIGH : LOW);
    digitalWrite(BACKLIGHT_LCD, state.substring(4, 5) == "0" ? HIGH : LOW);
  }
}

void adjustBacklights() {
  int blPower = map(analogRead(BACKLIGHT_POTENTIOMETER), 0, 1023, BACKLIGHT_MIN_DUTY, BACKLIGHT_MAX_DUTY);
  if(currentBacklightDuty != blPower) {
    currentBacklightDuty = blPower;
    analogWrite(BACKLIGHT_WB, backlightState.substring(0, 1) == "1" ? blPower : 0);
    analogWrite(BACKLIGHT_WS, backlightState.substring(1, 2) == "1" ? blPower : 0);
  }
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
    if(diff != 0) {
      diff = diff > 0 ? 1 : -1;
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
  updateGauges("0_0_0_0");
}

void gaugeHomeReset() {
  resetGauges();
  gaugeMotorA.step(MOTOR_BIG_STEPS * 1.5);
  gaugeMotorB.step(MOTOR_SMALL_STEPS * 1.5);
  gaugeMotorC.step(MOTOR_SMALL_STEPS * 1.5);
  gaugeMotorD.step(MOTOR_BIG_STEPS * 1.5);
  for(int i = 0; i < 4; i++) {
    gaugeCurrentStep[i] = 0;
    gaugeTargetStep[i] = 0;
  }
}

void updateRegistry(int id, String data) {
  String v[5];
  splitData(data, 5, v);
  switch(id) {
    case 1:
      for(int i = 0; i < 5; i++) {
        if(regA[i] != v[i]) regChanges[i] = true;
        regA[i] = v[i];
      }
      break;
    case 2:
      for(int i = 0; i < 5; i++) {
        if(regB[i] != v[i]) regChanges[5 + i] = true;
        regB[i] = v[i];
      }
      break;
    case 3:
      for(int i = 0; i < 5; i++) {
        if(regC[i] != v[i]) regChanges[10 + i] = true;
        regC[i] = v[i];
      }
      break;
  }
}

void setup() {
  Serial.begin(9600);
  //LCD
  tft.begin();
  tft.setRotation(0);
  w = tft.width();
  h = tft.height();
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
}

void loop() {
  if(serialDataIsReady) {
    String cmdType = serialBuffer.substring(0, 3);
    String cmdData = serialBuffer.substring(4, serialBuffer.length());
    if(cmdType == "LED"){
      updateLeds(cmdData);
    }else if(cmdType == "BKL") {
      updateBacklights(cmdData);
    }else if(cmdType == "GAU") {
      updateGauges(cmdData);
    }else if(cmdType == "GHR") {
      gaugeHomeReset();
    }else if(cmdType == "REA") {
      updateRegistry(1, cmdData);
    }else if(cmdType == "REB") {
      updateRegistry(2, cmdData);
    }else if(cmdType == "REC") {
      updateRegistry(3, cmdData);
    }else if(cmdType == "PRT") {
      printLcd(cmdData.toInt());
    }else if(cmdType == "UPD") {
      updateLcd(false);
    }
    serialDataIsReady = false;
    serialBuffer = "";
  }
  updateBlinkingLeds();
  adjustBacklights();
  moveMotors();
}

void serialEvent() {
  while(Serial.available() > 0) {
    char inChar = (char)Serial.read();
    if(inChar == '\n') {
      serialDataIsReady = true;
      break;
    }else{
      serialBuffer += inChar;
    }
  }
}
