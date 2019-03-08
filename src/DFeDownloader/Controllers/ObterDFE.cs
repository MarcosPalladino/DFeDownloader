using DFeDownloader.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using WebService = DFeDownloader.NFeDistribuicaoDFe.NFeDistribuicaoDFe;

namespace DFeDownloader.Controllers
{
    public static class ObterDFE
    {
        private static AppConfig _appconfig { get; set; }

        public static bool EfetuarDownload(AppConfig appConfig, string nsu = "", string chvnfe = "")
        {
            _appconfig = appConfig;

            var Return = false;
            Console.WriteLine(DateTime.Now + " Iniciando o processo de download de Documentos Fiscais SEFAZ - Empresa: "+_appconfig.Empresa+" LastNSU: "+_appconfig.LastNSU);
            XmlNode Noderequest, Noderesponse;
            // cria o xml para obter todos os nsu
            var xml = CreateRequestXml(nsu, chvnfe);
            // cria o webservice
            var distribuicaoDFe = CreateWebService("https://www1.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx?wsdl");

            //Transformando o XML em XmlNode (Requisito da API)
            Noderequest = xml.DocumentElement;

            //Console.WriteLine(DateTime.Now + " Iniciando consulta ao webservice.");
            //Envio ao WebService
            Noderesponse = distribuicaoDFe.nfeDistDFeInteresse(Noderequest);

            //Noderesponse = Noderequest.FirstChild.LastChild;
            var retorno = Noderesponse.ChildNodes;

            //Console.WriteLine(DateTime.Now + " Iniciando a gravação dos arquivos.");

            //Percorre todos os Nós do XML principal
            foreach (XmlNode loopNode in retorno)
            {
                if (loopNode.Name == "loteDistDFeInt")
                {
                    var xmls = loopNode.ChildNodes;
                    foreach (XmlElement item in xmls)
                    {
                        Return = GravarXml(item.InnerText, item.Attributes["NSU"].Value, item.Attributes["schema"].Value);
                    }
                }
                if (loopNode.Name == "ultNSU")
                {
                    _appconfig.LastNSU = loopNode.InnerText;
                }

            }
            Console.WriteLine(DateTime.Now + " Fim do processamento. LastNSU: "+_appconfig.LastNSU);
            Console.WriteLine();
            return Return;
        }
        public static WebService CreateWebService(string url)
        {
            //Console.WriteLine(DateTime.Now + " Preparando conexao com o webservice");
            var retorno = new WebService();

            var objCertificadoX509 = LoadCertificate();
            //Setando os Atributos de Certificado e endereço de envio
            retorno.ClientCertificates.Add(objCertificadoX509);
            retorno.Url = url;

            return retorno;
        }
        public static XmlDocument CreateRequestXml(string nsu = "", string chvnfe = "")
        {
            //Console.WriteLine(DateTime.Now + " Preparando requisição SOAP");
            string Xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                         "<distDFeInt versao=\"1.01\" xmlns=\"http://www.portalfiscal.inf.br/nfe\">" +
                             "<tpAmb>1</tpAmb>" +
                             "<cUFAutor>" + _appconfig.UF + "</cUFAutor>" +
                             "<CNPJ>" + _appconfig.CNPJ + "</CNPJ>";

            if (string.IsNullOrEmpty(nsu) && string.IsNullOrEmpty(chvnfe))
            {
                Xml += "<distNSU>" +
                            "<ultNSU>"+_appconfig.LastNSU+"</ultNSU>" +
                        "</distNSU>";

            }
            else if (!string.IsNullOrEmpty(chvnfe))
            {
                Xml += "<consChNFe>" +
                            "<chNFe>" + chvnfe + "</chNFe>" +
                        "</consChNFe>";
            }
            else
            {
                Xml += "<consNSU>" +
                        "<NSU>" + nsu + "</NSU>" +
                    "</consNSU>";
            }
            Xml += "</distDFeInt>";

            var doc = new XmlDocument();
            doc.LoadXml(Xml);

            return doc;
        }
        public static bool GravarXml(string xml, string nsu, string schema)
        {
            var compactado = xml;
            var descompactado = GzipDecodeWW(compactado);
            var nome = "";
            var chNFE = "";
            var pos = descompactado.IndexOf("chNFe");
            if (pos > 0)
            {
                chNFE = descompactado.Substring(pos + 6, 44);
            }
            if (!schema.Contains("procNFe"))
            {
                //Console.WriteLine("NFe não disponível: " + chNFE + " NSU " + nsu + " Schema " + schema);
                return false;
            }
            nome += chNFE;
            if (!string.IsNullOrEmpty(nome))
            {
                nome += "_";
            }

            nome += nsu;
            nome += "_" + schema;

            File.WriteAllText(_appconfig.XmlFolder + nome + ".xml", descompactado);
            Console.WriteLine(DateTime.Now + " "+_appconfig.XmlFolder + nome + ".xml");
            return true;
        }
        private static string GzipDecodeWW(string inputStr)
        {
            byte[] inputBytes = Convert.FromBase64String(inputStr);

            using (var inputStream = new MemoryStream(inputBytes))
            using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gZipStream.CopyTo(outputStream);
                var outputBytes = outputStream.ToArray();

                string decompressed = Encoding.UTF8.GetString(outputBytes);

                return decompressed;
            }
        }
        public static X509Certificate2 LoadCertificate()
        {
            var caminho = @"./certificado/certificado.pfx";
            //Console.WriteLine(DateTime.Now + " Importando certificado digital " + caminho);
            return new X509Certificate2(caminho, _appconfig.Password);
        }
        public static void CreateIfMissing(string path)
        {
            bool folderExists = Directory.Exists(path);
            if (!folderExists)
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}