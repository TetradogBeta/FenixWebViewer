from os.path import exists
from Capitulo import Capitulo
import os
import numpy
import time
import telegram
import TelegramHelper


class Checker(object):
    CheckLog="@CheckingLog";

    def __init__(self,configFileName="config"):
        self.ConfigFileName=configFileName;
        self.TotalLoop =-1;
        self.TotalLoopLoaded=False;
        self.Bot=None;
        self.ChatLogId=-1;
        self.ChatLogIdLoaded=False;
        self.DicCapitulos={};

    def Load(self,args):
        if len(args)>=4:
            self.Web=args[0];
            self.Channel=args[1];
            self.ApiBotKey=args[2];
            if len(args)>3:
                self.TotalLoop=args[3];
            if len(args)>4:
                self.ChatLogId=args[4];
                self.ChatLogIdLoaded=True;

        elif exists(self.ConfigFileName):
            fConfig = open(self.ConfigFileName, "r");
            config = fConfig.readlines();
            fConfig.close();
            self.Web=config[0].replace("\n","");
            self.Channel=config[1].replace("\n","");
            self.ApiBotKey=config[2].replace("\n","");
            if len(config)>4:
                self.TotalLoop=config[3].replace("\n","");
                self.TotalLoopLoaded=True;
            if len(config)>=5:
                self.ChatLogId=config[4].replace("\n","");
                self.ChatLogIdLoaded=True;

        else:
            raise Exception("Se necesita informacion para iniciar el BOT!");

        if len(args)>1:
            self.TotalLoop=args[0];

        self.UpdateConfig();

        if not isinstance(self.TotalLoop, int):
            self.TotalLoop=int(self.TotalLoop);
        if not isinstance(self.ChatLogId, int):
            self.ChatLogId=int(self.ChatLogId);
        if not self.Channel.startswith("@"):
            self.Channel="@"+str(self.Channel);

    def UpdateConfig(self):
        if exists(self.ConfigFileName): 
            os.remove(self.ConfigFileName);
        config=[self.Web+"\n",self.Channel+"\n",self.ApiBotKey+"\n"];
        fConfig = open(self.ConfigFileName, 'w');
        if(self.TotalLoopLoaded):
            config.append(str(self.TotalLoop)+"\n");
        else:
            config.append("-1\n");    
        if(self.ChatLogIdLoaded):
            config.append(str(self.ChatLogId)+"\n");
        fConfig.writelines(config);
        fConfig.close();

    async def InitUpdate(self):
        if self.Bot is None:
            self.Bot=telegram.Bot(self.ApiBotKey);
            self.ChatChannel=self.Bot.getChat(self.Channel);
            if self.ChatLogId>=0:
                messageLog=self.GetMessageLog();
                if "\n" in messageLog:
                    posts=messageLog.split('\n');
                    for postId in posts:
                        if postId!='' and Checker.IsNumber(postId) and int(postId)>=0:
                            try:
                                post=TelegramHelper.GetMessage(self.ChatChannel,postId);
                                capitulo=post.split('\n')[0];
                                #elimino los &fff;
                                while "&" in capitulo:
                                    indexAnd=capitulo.index("&");
                                    if indexAnd!=-1:
                                        indexSemicolon=capitulo.index(";",indexAnd);
                                        capitulo=capitulo[:indexAnd]+capitulo[indexSemicolon+1:];
                                if capitulo.endswith(' '):
                                    capitulo=capitulo[:-1];
                                if capitulo not in self.DicCapitulos:
                                    self.DicCapitulos[capitulo]=postId;
                                else:
                                    self.Bot.delete_message(self.Channel,postId);
                                    print("Duplicated "+str(capitulo));
                            except:
                                print("Error--"+postId);
                    self.UpdateLog();
            else:
                message=self.Bot.send_message(Checker.CheckLog,"Init "+str(self.Bot.username));
                self.ChatLogId=message.message_id;
                self.ChatLogIdLoaded=True;
                self.UpdateConfig();
                
    def GetMessageLog(self):
        self.ChatLog=self.Bot.getChat(Checker.CheckLog);
        return TelegramHelper.GetMessage(self.ChatLog,self.ChatLogId);

    async def Update(self):
        init=True;
        hasAnError=False;
        
        if self.TotalLoop>=0:
            for i in range(0,self.TotalLoop):
                    self._WaitDescanso(init,hasAnError);
                    hasAnError=self._WaitAnError(await self.OneLoopUpdate());   
                    init=False;
        else:
            while True:
                self._WaitDescanso(init,hasAnError);
                hasAnError=self._WaitAnError(await self.OneLoopUpdate());  
                init=False;

    def _WaitAnError(self,hasAnError):
        if hasAnError:
            print("Ha habido un problema, vuelvo a intentarlo en 10 segundos");
            time.sleep(10);
        return hasAnError;
    def _WaitDescanso(self,init,hasAnError):
        if not init and not hasAnError:
            print("Descanso de 5 min");
            time.sleep(5*60);

    
    async def OneLoopUpdate(self):
        await self.InitUpdate();
        try:
            for capitulo in Capitulo.GetCapitulos(self.Web):
                nombreCapitulo=capitulo.Name;
                if "!" in nombreCapitulo:
                    nombreCapitulo=nombreCapitulo.replace("!","");

                if nombreCapitulo not in self.DicCapitulos:
                    print(capitulo.Name);
                    self.DicCapitulos[nombreCapitulo]=self.Bot.send_photo(self.ChatChannel.id, capitulo.Picture,capitulo.Name+"\n"+capitulo.GetLinkMega()).message_id;
                    self.UpdateLog();
                    
            hasAnError=False;
        except:
            hasAnError=True;
        return hasAnError;
        
    def UpdateLog(self):
        strMessageLog="";
        for idPost in self.DicCapitulos.values():
            if int(idPost)>=0:
                strMessageLog+=str(idPost)+"\n";
        try:        
            self.Bot.edit_message_text(strMessageLog,Checker.CheckLog,self.ChatLogId);
        except:
            pass;




    @staticmethod #https://www.geeksforgeeks.org/implement-isnumber-function-in-python/
    # Implementation of isNumber() function
    def IsNumber(s):
        
        # handle for negative values
        isNum=s!=None and s!="";
        if isNum:
            negative = False
            if(s[0] =='-'):
                negative = True;
                
            if negative == True:
                s = s[1:];
            
            # try to convert the string to int
            try:
                dummy = int(s)
                isNum= True;
            # catch exception if cannot be converted
            except ValueError:
                isNum= False;
        return isNum;
        

