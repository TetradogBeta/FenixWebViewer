from sys import dont_write_bytecode
from bs4 import BeautifulSoup
import requests
import re


class Capitulo(object):
    """Representa a un capitulo de una serie"""
    def __init__(self,nodoCapitulo):
        soup = BeautifulSoup(str(nodoCapitulo), "html.parser");
        htmlA=soup.find("a");
        htmlImg=soup.find("img");
        self.Picture=htmlImg.get("src");
        self.Name=htmlA.get("title");
        self.Pagina=htmlA.get("href");
    def GetLinks(self):
        regex=re.compile("(<iframe[^>]*>(.*?)</iframe>)");
        page = requests.get(self.Pagina);
        htmlPage=str(page.content);
        for iframeHtml in regex.findall(htmlPage):
            soup = BeautifulSoup(iframeHtml[0], "html.parser");
            iframe=soup.find("iframe");
            url= iframe.get("src").replace("&amp;","&").replace("\\'","");
            page=requests.get(url);
            soup=BeautifulSoup(regex.search(str(page.content)).group(), "html.parser");
            iframe=soup.find("iframe");
            yield iframe.get("src").replace("&amp;","&");

    def GetLinkMega(self):
        MARCAFIN="#FIN#"; 
        linksGen=self.GetLinks();
        link=next(linksGen,MARCAFIN);
        linkMega=None;
        while link != MARCAFIN and linkMega==None:
            if "mega" in link:
                linkMega= link.replace("embed","file");
            link=next(linksGen,MARCAFIN);
        return linkMega;
    @staticmethod
    def GetNodosCapitulos(uriWeb):
        page = requests.get(uriWeb);
        soup = BeautifulSoup(page.content, "html.parser");
        gridCapitulos=soup.find("div","capitulos-grid").children;
    
        for item in gridCapitulos:
            if '\n' != item:
                yield item;
            

    @staticmethod
    def GetCapitulos(uriWeb):
        for nodoCapitulo in Capitulo.GetNodosCapitulos(uriWeb):
            yield  Capitulo(nodoCapitulo);




