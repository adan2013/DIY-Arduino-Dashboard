#include <Wire.h>
#include <Adafruit_MCP23017.h>

Adafruit_MCP23017 mcp;

void setup() {
  mcp.begin();
  for(int i = 0; i < 16; i++) {
    mcp.pinMode(i, OUTPUT);
    mcp.digitalWrite(i, LOW);
  }
}

void loop() {
  for(int i = 0; i < 16; i++) {
    int prev = i - 1;
    if(prev < 0) prev = 15;
    mcp.digitalWrite(prev, LOW);
    mcp.digitalWrite(i, HIGH);
    delay(500);
  }
}
