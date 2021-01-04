void updateLcd() {
  int w = tft.width(), h = tft.height();
  switch(screenId) {
    case 1: //TESTING
      if(clearValuesRequired) clearLcd();
      tft.fillScreen(regB[0] == "0" ? ILI9341_RED : regB[0] == "1" ? ILI9341_GREEN : regB[0] == "2" ? ILI9341_BLUE : ILI9341_WHITE);
      break;
    case 3: //ASSISTANT
      if(clearValuesRequired) { clearParam(1); clearParam(3); clearParam(5); clearParam(7); }
      printParam(1, regB[1], true);
      printParam(3, regB[2], true);
      printParam(5, regB[3], true);
      printParam(7, regB[4], true);
      break;
    case 4: //NAVIGATION
    case 5: //JOB
      if(clearValuesRequired) { clearParam(1); clearParam(3); clearParam(5); }
      printParam(1, regB[0], true);
      printParam(3, regB[1], true);
      printParam(5, regB[2], true);
      break;
    case 6: //ENGINE
      if(clearValuesRequired) { clearParam(1); clearParam(3); clearParam(5); clearParam(7); }
      printParam(1, regB[0], true);
      printParam(3, regB[1], false);
      printParam(3, regB[2], true);
      printParam(5, regB[3], true);
      printParam(7, regB[4], true);
      break;
    case 7: //FUEL
      if(clearValuesRequired) { clearParam(1); clearParam(3); clearParam(5); clearParam(7); }
      printParam(1, regB[1] + "/" + regB[1], true);
      printParam(3, regB[2], true);
      printParam(5, regB[3], true);
      printParam(7, regB[4], true);
      break;
    case 8: //TRUCK
      if(clearValuesRequired) clearCenterScreen();
      for(int i = 0; i < 5; i++) {
        int dmg = regB[i].toInt();
        uint16_t color = ILI9341_DARKGREEN;
        if(dmg >= 3 && dmg < 6) { color = ILI9341_GREEN;
        }else if(dmg >= 6 && dmg < 9) { color = ILI9341_YELLOW;
        }else if(dmg >= 9 && dmg < 12) { color = ILI9341_ORANGE;
        }else if(dmg >= 12) { color = ILI9341_RED; }
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
      break;
    case 9: //TRAILER
      if(clearValuesRequired) { clearParam(1); clearParam(3); clearParam(5); clearParam(7); }
      int dmg = regB[0].toInt();
      uint16_t color = ILI9341_DARKGREEN;
      if(dmg >= 3 && dmg < 6) { color = ILI9341_GREEN;
      }else if(dmg >= 6 && dmg < 9) { color = ILI9341_YELLOW;
      }else if(dmg >= 9 && dmg < 12) { color = ILI9341_ORANGE;
      }else if(dmg >= 12) { color = ILI9341_RED; }
      tft.fillRect(45, 65, 155, 40, color);
      tft.fillRect(100, 105, 32, 5, color);
      for(int i = 0; i < 3; i++) {
        tft.fillCircle(145 + i * 20, 109, 9, ILI9341_BLACK);
        tft.fillCircle(145 + i * 20, 109, 7, color);
      }
      printParam(3, regB[2], false);
      printParam(4, regB[0] + "%", true);
      printParam(5, regB[3], true);
      printParam(6, regB[1], true);
      printParam(7, regB[4], true);
      break;
  }
  clearValuesRequired = true;
}
