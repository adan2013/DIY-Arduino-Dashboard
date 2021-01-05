void updateLcd() {
  int w = tft.width(), h = tft.height();
  switch(screenId) {
    case 1: //TESTING
    {
      if(clearValuesRequired) clearLcd();
      tft.fillScreen(regB[0] == "0" ? ILI9341_RED : regB[0] == "1" ? ILI9341_GREEN : regB[0] == "2" ? ILI9341_BLUE : ILI9341_WHITE);
    }
      break;
    case 3: //ASSISTANT
    {
      if(clearValuesRequired) { clearParam(1); clearParam(3); clearParam(5); clearParam(7); }
      printParam(1, regB[1], true);
      printParam(3, regB[2], true);
      printParam(5, regB[3], true);
      printParam(7, regB[4], true);
    }
      break;
    case 4: //NAVIGATION
    case 5: //JOB
    {
      if(clearValuesRequired) { clearParam(1); clearParam(3); clearParam(5); }
      printParam(1, regB[0], true);
      printParam(3, regB[1], true);
      printParam(5, regB[2], true);
    }
      break;
    case 6: //ENGINE
    {
      if(clearValuesRequired) { clearParam(1); clearParam(3); clearParam(5); clearParam(7); }
      printParam(1, regB[0], true);
      printParam(3, regB[1], false);
      printParam(3, regB[2], true);
      printParam(5, regB[3], true);
      printParam(7, regB[4], true);
    }
      break;
    case 7: //FUEL
    {
      if(clearValuesRequired) { clearParam(1); clearParam(3); clearParam(5); clearParam(7); }
      printParam(1, regB[1] + "/" + regB[1], true);
      printParam(3, regB[2], true);
      printParam(5, regB[3], true);
      printParam(7, regB[4], true);
    }
      break;
    case 8: //TRUCK
    {
      if(clearValuesRequired) clearCenterScreen();
      for(int i = 0; i < 5; i++) {
        int dmg = regB[i].toInt();
        uint16_t color = getDiagnosticColor(dmg);
        switch(i) {
          case 0: //cabin
            tft.fillRect(20, 70, 80, 105, color);
            break;
          case 1: //chassis
            tft.fillRect(20, 175, 200, 40, color);
            break;
          case 2: //engine
            tft.fillRect(32, 167, 45, 28, ILI9341_BLACK);
            tft.fillRect(34, 169, 41, 24, color);
            break;
          case 3: //transmission
            tft.fillRect(75, 179, 35, 16, ILI9341_BLACK);
            tft.fillRect(75, 181, 33, 12, color);
            break;
          case 4: //wheels
            tft.fillCircle(55, 217, 20, ILI9341_BLACK);
            tft.fillCircle(55, 217, 16, color);
            tft.fillCircle(145, 217, 20, ILI9341_BLACK);
            tft.fillCircle(145, 217, 16, color);
            tft.fillCircle(190, 217, 20, ILI9341_BLACK);
            tft.fillCircle(190, 217, 16, color);
            break;
        }
        tft.setCursor(w - 120, 62 + i * 22);
        tft.println(getTruckPartName(i) + dmg + "%");
      }
    }
      break;
    case 9: //TRAILER
    {
      if(clearValuesRequired) clearCenterScreen();
      uint16_t color = getDiagnosticColor(regB[0].toInt());
      tft.fillRect(45, 65, 155, 40, color);
      tft.fillRect(100, 105, 32, 5, color);
      for(int i = 0; i < 3; i++) {
        tft.fillCircle(145 + i * 20, 109, 9, ILI9341_BLACK);
        tft.fillCircle(145 + i * 20, 109, 7, color);
      }
      printParam(4, "Damage", false);
      printParam(5, "Mass", false);
      printParam(6, "Lift axle", false);
      printParam(7, "Attached", false);
      tft.setTextWrap(false);
      printParam(3, regB[2], false);
      tft.setTextWrap(true);
      printParam(4, regB[0] + "%", true);
      printParam(5, regB[3], true);
      printParam(6, regB[1], true);
      printParam(7, regB[4], true);
    }
      break;
    case 10: //MAIN MENU
    {
      printMenuItem(0, "Back", regB[0] == "0");
      printMenuItem(1, "Settings", regB[0] == "1");
      printMenuItem(2, "Customization", regB[0] == "2");
      printMenuItem(3, "Acceleration", regB[0] == "3");
      printMenuItem(4, "Information", regB[0] == "4");
      printMenuNavHint(regB[0] == "0" ? "L/R-move | OK-exit" : "L/R-move | OK-select");
    }
      break;
    case 11: //SETTINGS
    {
      if(clearValuesRequired) clearParam(8);
      printMenuItem(0, "Back", regB[0] == "0");
      printMenuItem(1, "Sound", regB[0] == "1");
      printMenuItem(2, "Clock 24h", regB[0] == "2");
      printMenuItem(3, "Real time clock", regB[0] == "3");
      printMenuItem(4, "Eco shift", regB[0] == "4");
      printMenuItem(5, "Speed limit warn", regB[0] == "5");
      if(regB[0] != "0") {
        tft.setTextColor(ILI9341_YELLOW);
        printParam(8, "Current: " + regB[1], false);
        tft.setTextColor(ILI9341_WHITE);
      }
      printMenuNavHint(regB[0] == "0" ? "L/R-move | OK-exit" : "L/R-move | OK-change");
    }
      break;
    case 12: //CUSTOMIZATION
    {
      if(clearValuesRequired) { clearParam(2); clearParam(5); }
      String optName = "Initial image";
      if(regB[0] == "1") { optName = "Assistant 1";
      }else if(regB[0] == "2") { optName = "Assistant 2";
      }else if(regB[0] == "3") { optName = "Assistant 3";
      }else if(regB[0] == "4") { optName = "Assistant 4"; }
      tft.setTextColor(ILI9341_YELLOW);
      printParam(2, optName, true);
      printParam(5, regB[1], true);
      tft.setTextColor(ILI9341_WHITE);
      printMenuNavHint(regB[0] == "4" ? "L/R-change | OK-exit" : "L/R-change | OK-next option");
    }
      break;
    case 13: //ACCELERATION
    {
      if(clearValuesRequired) { clearParam(1); clearParam(4); clearParam(6); clearParam(8); }
      tft.setTextColor(ILI9341_YELLOW);
      switch(regB[0].toInt()) {
        case 0: //select target speed
          printParam(1, "Select t. speed", false);
          tft.setTextColor(ILI9341_WHITE);
          printParam(4, regB[1] + "km/h", true);
          tft.setTextColor(ILI9341_YELLOW);
          printParam(6, regB[2] + "km/h", true);
          tft.setTextColor(ILI9341_WHITE);
          printParam(8, "--:--:---", true);
          printMenuNavHint("L/R-change | OK-confirm");
          break;
        case 1: //waiting for stop
          printParam(1, "Stop the truck", false);
          tft.setTextColor(ILI9341_WHITE);
          printParam(4, regB[1] + "km/h", true);
          printParam(6, regB[2] + "km/h", true);
          printParam(8, "--:--:---", true);
          printMenuNavHint("OK-cancel");
          break;
        case 2: //waiting for start
          printParam(1, "Ready to go", false);
          tft.setTextColor(ILI9341_WHITE);
          printParam(4, regB[1] + "km/h", true);
          printParam(6, regB[2] + "km/h", true);
          printParam(8, "--:--:---", true);
          printMenuNavHint("OK-canel");
          break;
        case 3: //measuring
          printParam(1, "MEASURING...", false);
          printParam(4, regB[1] + "km/h", true);
          tft.setTextColor(ILI9341_WHITE);
          printParam(6, regB[2] + "km/h", true);
          printParam(8, "--:--:---", true);
          printMenuNavHint("OK-cancel");
          break;
        case 4: //time display
          printParam(1, "Finished!", false);
          tft.setTextColor(ILI9341_WHITE);
          printParam(4, regB[1] + "km/h", true);
          printParam(6, regB[2] + "km/h", true);
          tft.setTextColor(ILI9341_YELLOW);
          printParam(8, regB[3], true);
          tft.setTextColor(ILI9341_WHITE);
          printMenuNavHint("OK-exit");
          break;
      }
    }
      break;
  }
  clearValuesRequired = true;
}
