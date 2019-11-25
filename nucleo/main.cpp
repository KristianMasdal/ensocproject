//Modifisert og hentet fra https://github.com/rlangoy/socLabWeek42/tree/master/mbed
#include "mbed.h"
#include "TextLCD.h"

TextLCD lcd(D11,D10,D9,D5,D4,D3,D2);
int main()
{
    char buffer[80]; // Mesage to be displayed on the  LCD-Module
    lcd.printf("Waiting for: "); 
    lcd.gotoxy(1,2);  
    lcd.printf("$LCD,Line1,Line2"); 
 
    while(1) {        
        scanf("%[^\r\n]s",buffer);  // Read a whole line 
        getchar();                             // remove "\r\n" from stio
        
       //vars used to split the CSV string
        int count = 0;  // counter
        char* tok;      // holds the string element                
        
        tok = strtok(buffer, ",");   // get the first element in string before the ","
        if (strcmp(tok,"$LCD")==0)
        {
            lcd.lcdComand(0x01);                 // Clear the display
            wait_ms(2);

            while(tok != NULL) 
            {
                tok = strtok(NULL, ",");      // get the next token (next string item) 
                count++;                      // inc counter
                lcd.gotoxy(1,count);          // Move to the correct line
                wait_ms(3);
                lcd.printf("%s",tok);     // Display token                
                wait_ms(3);               
            } 
        }   
    }
}





