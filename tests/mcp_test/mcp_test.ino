#include <Wire.h>
#include <Adafruit_MCP23017.h>

Adafruit_MCP23017 mcp;

void setup() {
  mcp.begin();
  mcp.pinMode(8, OUTPUT);
  mcp.digitalWrite(8, LOW);
}

void loop() {
  delay(500);
  mcp.digitalWrite(8, HIGH);
  delay(500);
  mcp.digitalWrite(8, LOW);
}
