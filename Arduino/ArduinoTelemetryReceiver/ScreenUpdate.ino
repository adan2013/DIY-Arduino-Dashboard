void printAccParam0(bool firstPrint, String text) {
  if(firstPrint || regChanges[5]) {
    if(!firstPrint) clearParam(1);
    printParam(1, text, false);
  }
}

void printAccParam1(bool firstPrint, String speedValue) {
  if(firstPrint || regChanges[6]) {
    if(!firstPrint) clearParam(4);
    printParam(4, speedValue + "km/h", true);
  }
}

void printAccParam2(bool firstPrint, String targetSpeedValue) {
  if(firstPrint || regChanges[7]) {
    if(!firstPrint) clearParam(6);
    printParam(6, targetSpeedValue + "km/h", true);
  }
}

void printAccParam3(bool firstPrint, String timeValue) {
  if(firstPrint || regChanges[8]) {
    if(!firstPrint) clearParam(8);
    printParam(8, timeValue == "" ? "--:--:---" : timeValue, true);
  }
}

void updateLcd(bool firstPrint) {
  printBars(firstPrint);
  if(!(firstPrint || regBChanged())) return;
  switch(screenId) {
    case 1: //TESTING
    {
      if(firstPrint || regChanges[5]) {
        if(!firstPrint) clearLcd();
        tft.fillScreen(regB[0] == "0" ? ILI9341_RED : regB[0] == "1" ? ILI9341_GREEN : regB[0] == "2" ? ILI9341_BLUE : ILI9341_WHITE);
      }
    }
      break;
    case 3: //ASSISTANT
    {
      if(firstPrint || regChanges[6]) {
        if(!firstPrint) clearParam(1);
        printParam(1, regB[1], true);
      }
      if(firstPrint || regChanges[7]) {
        if(!firstPrint) clearParam(3);
        printParam(3, regB[2], true);
      }
      if(firstPrint || regChanges[8]) {
        if(!firstPrint) clearParam(5);
        printParam(5, regB[3], true);
      }
      if(firstPrint || regChanges[9]) {
        if(!firstPrint) clearParam(7);
        printParam(7, regB[4], true);
      }
    }
      break;
    case 4: //NAVIGATION
    case 5: //JOB
    {
      if(firstPrint || regChanges[5]) {
        if(!firstPrint) clearParam(1);
        printParam(1, regB[0], true);
      }
      if(firstPrint || regChanges[6]) {
        if(!firstPrint) clearParam(3);
        printParam(3, regB[1], true);
      }
      if(firstPrint || regChanges[7]) {
        if(!firstPrint) clearParam(5);
        printParam(5, regB[2], true);
      }
    }
      break;
    case 6: //ENGINE
    {
      if(firstPrint || regChanges[5]) {
        if(!firstPrint) clearParam(1);
        printParam(1, regB[0], true);
      }
      if(firstPrint || regChanges[6] || regChanges[7]) {
        if(!firstPrint) clearParam(3);
        printParam(3, regB[1], false);
        printParam(3, regB[2], true);
      }
      if(firstPrint || regChanges[8]) {
        if(!firstPrint) clearParam(5);
        printParam(5, regB[3], true);
      }
      if(firstPrint || regChanges[9]) {
        if(!firstPrint) clearParam(7);
        printParam(7, regB[4], true);
      }
    }
      break;
    case 7: //FUEL
    {
      if(firstPrint || regChanges[5] || regChanges[6]) {
        if(!firstPrint) clearParam(1);
        printParam(1, regB[0] + "/" + regB[1], true);
      }
      if(firstPrint || regChanges[7]) {
        if(!firstPrint) clearParam(3);
        printParam(3, regB[2], true);
      }
      if(firstPrint || regChanges[8]) {
        if(!firstPrint) clearParam(5);
        printParam(5, regB[3], true);
      }
      if(firstPrint || regChanges[9]) {
        if(!firstPrint) clearParam(7);
        printParam(7, regB[4], true);
      }
    }
      break;
    case 8: //TRUCK
    {
      if(!firstPrint) clearCenterScreen();
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
      if(!firstPrint) clearCenterScreen();
      uint16_t color = getDiagnosticColor(regB[0].toInt());
      tft.fillRect(45, 65, 155, 40, color);
      tft.fillRect(100, 105, 32, 5, color);
      for(int i = 0; i < 3; i++) {
        tft.fillCircle(145 + i * 20, 109, 9, ILI9341_BLACK);
        tft.fillCircle(145 + i * 20, 109, 7, color);
      }
      tft.setTextColor(ILI9341_LIGHTGREY);
      printParam(4, "Damage", false);
      printParam(5, "Mass", false);
      printParam(6, "Lift axle", false);
      printParam(7, "Attached", false);
      tft.setTextColor(ILI9341_WHITE);
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
      if(!firstPrint) clearParam(8);
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
      if(!firstPrint) { clearParam(2); clearParam(5); }
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
      tft.setTextColor(ILI9341_YELLOW);
      switch(regB[0].toInt()) {
        case 0: //select target speed
          printAccParam0(firstPrint, "Select t. speed");
          tft.setTextColor(ILI9341_WHITE);
          printAccParam1(firstPrint, regB[1]);
          tft.setTextColor(ILI9341_YELLOW);
          printAccParam2(firstPrint, regB[2]);
          tft.setTextColor(ILI9341_WHITE);
          printAccParam3(firstPrint, "");
          printMenuNavHint("L/R-change | OK-confirm");
          break;
        case 1: //waiting for stop
          printAccParam0(firstPrint, "Stop the truck");
          tft.setTextColor(ILI9341_WHITE);
          printAccParam1(firstPrint, regB[1]);
          printAccParam2(firstPrint, regB[2]);
          printAccParam3(firstPrint, "");
          printMenuNavHint("OK-cancel");
          break;
        case 2: //waiting for start
          printAccParam0(firstPrint, "Ready to go");
          tft.setTextColor(ILI9341_WHITE);
          printAccParam1(firstPrint, regB[1]);
          printAccParam2(firstPrint, regB[2]);
          printAccParam3(firstPrint, "");
          printMenuNavHint("OK-canel");
          break;
        case 3: //measuring
          printAccParam0(firstPrint, "MEASURING...");
          printAccParam1(firstPrint, regB[1]);
          tft.setTextColor(ILI9341_WHITE);
          printAccParam2(firstPrint, regB[2]);
          printAccParam3(firstPrint, "");
          printMenuNavHint("OK-cancel");
          break;
        case 4: //time display
          printAccParam0(firstPrint, "Finished!");
          tft.setTextColor(ILI9341_WHITE);
          printAccParam1(firstPrint, regB[1]);
          printAccParam2(firstPrint, regB[2]);
          tft.setTextColor(ILI9341_YELLOW);
          printAccParam3(firstPrint, regB[3]);
          tft.setTextColor(ILI9341_WHITE);
          printMenuNavHint("OK-exit");
          break;
      }
    }
      break;
  }
  regBUpdated();
}
