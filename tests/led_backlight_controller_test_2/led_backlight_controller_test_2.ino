#include <Wire.h>
#include <Adafruit_MCP23017.h>

Adafruit_MCP23017 mcp;

void setup() {
  //LED
  mcp.begin();
  for(int i = 0; i < 16; i++) {
    mcp.pinMode(i, OUTPUT);
    mcp.digitalWrite(i, LOW);
  }
  //BACKLIGHT
  pinMode(8, OUTPUT);
  pinMode(9, OUTPUT);
  pinMode(10, OUTPUT);
  pinMode(11, OUTPUT);
  analogWrite(8, 50);
  analogWrite(9, 50);
  digitalWrite(10, HIGH);
  digitalWrite(11, HIGH);
}

void loop() {
  for(int i = 0; i < 16; i++) {
    int prev = i - 1;
    if(prev < 0) prev = 15;
    mcp.digitalWrite(prev, LOW);
    mcp.digitalWrite(i, HIGH);
    delay(800);
  }
}
