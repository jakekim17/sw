using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


using sw.swwebdataServiceReference;
using sw.threetempoServiceReference;
using sw.wowtvServiceReference;
using sw.wowtvSAServiceReference;
using sw.mobileSmsServiceReference;
using sw.KvinaServiceReference;


using System.Net;
using System.Text;
using System.IO;

using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;

namespace sw.Controllers
{
    public class HomeController : Controller
	{
		IswwebdataServiceClient client = new IswwebdataServiceClient();

		IwowtvServiceClient wowtvClient = new IwowtvServiceClient();

		IwowtvSAServiceClient wowtvSAClient = new IwowtvSAServiceClient();

		ImobileSmsServiceClient mobileSmsClient = new ImobileSmsServiceClient();

		public ActionResult test()
		{
			var resultData = client.test();
			
			return View(resultData);
		}

		public ActionResult Message()
		{
			//var resultData = wowtvClient.Message();

			ViewBag.Message = wowtvClient.Message();

			return View();
		}

		public ActionResult tblMobileAppDownList()
		{
			var resultData = wowtvClient.tblMobileAppDownList();
			ViewBag.seq = resultData.seq;
			ViewBag.mobile = resultData.mobile;

			return View();
		}


		public ActionResult tblMobileAppDownListSA()
		{
			var resultData = wowtvSAClient.tblMobileAppDownList();
			ViewBag.seq = resultData.seq;
			ViewBag.mobile = resultData.mobile;

			return View();
		}

		public ActionResult USP_SC_TRAN_INSERT(string mobile, string smsContents, string MsgID, string strLogInID)
		{			

			bool isSuccess = false;


			if (!string.IsNullOrEmpty(mobile) && !string.IsNullOrEmpty(smsContents))
			{
				
				mobileSmsClient.USP_SC_TRAN_INSERT(mobile, smsContents, MsgID, strLogInID);
				isSuccess = true;

				return Content("<script type=\"text/javascript\">alert(\"SP가 정상적으로 수행됐습니다.\"); history.back();</script>");
			}
			else
			{
				isSuccess = false;

				return Content("<script type=\"text/javascript\">alert(\"잘못된 접근입니다.\"); history.back();</script>");
			}

		}


		public ActionResult usp_MobileAppDownSMS(string mobile, string appName)
		{
			bool isSuccess = false;
			string msg = "";

			if (!string.IsNullOrEmpty(mobile) && !string.IsNullOrEmpty(appName))
			{
				wowtvClient.usp_MobileAppDownSMS(mobile, appName);
				isSuccess = true;
				msg = "SP가 정상적으로 수행됐습니다.";
			}
			else
			{
				isSuccess = false;
				msg = "adminid가 빈값이거나 NULL입니다.";
			}

			return Json(new { IsSuccess = isSuccess, Msg = msg }, JsonRequestBehavior.AllowGet);
		}
		
		public ActionResult cookie()
		{
			//생성방법1
			//HttpCookie cookie = new HttpCookie("UserId", "testid");
			//Response.Cookies.Add(cookie);

			//생성방법2
			Response.Cookies["UserId"].Value = "ckato21";
			Response.Cookies["UserId"].Expires = DateTime.Now.AddDays(1);	//기간을 지정하면 기간동안 쿠키가 저장된다. 

			//호출방법
			//return Content("<script type=\"text/javascript\">alert(\"" + cookie + "\");</script>");
			return Content("<script type=\"text/javascript\">alert(\"" + Request.Cookies["UserId"].Value + "\");</script>");

		}


		//로그인성공시, 아래 경로 호출하게 되어있음.
		public ActionResult LoginProc(string devk, string returnCode)
		{
			if (returnCode == "error" || returnCode == "noapp")
			{
				if (returnCode == "noapp" )
				{					
					return Content("<script type=\"text/javascript\">alert(\"주식창 설치 후 로그인 가능합니다.주식창 설치 문자를 확인하세요.\"); history.back();</script>");
				}
				else if (returnCode == "error")
				{
					return Content("<script type=\"text/javascript\">alert(\"잘못된 접근입니다. 휴대폰번호를 다시 확인하거나, 알람 수신설정을 확인하세요.\"); history.back();</script>");
				}
			}
			else
			{
				//devk = "61a59f76-6cdf-4fc0-8542-1fc30279583b";
			}
			Response.Cookies["devk"].Value = devk;

			//Response.Cookies["nickname"].Value = "코스피걸";
			Response.Cookies["nickname"].Value = "KospiGirl";

			return Redirect("/");
		}

		//로그아웃 요청시 아래 경로 호출.
		public ActionResult LogoutProc()
		{
			Response.Cookies["devk"].Value = null;
			Response.Cookies["nickname"].Value = null;

			return Redirect("/");
		}

		//sw1서버에서는 아래 경로 접근가능함. 내PC에서는 안됨. 이런 이유로 사용하지 않음.
		public ActionResult Login(string phno)
		{
			string result_descript = "";

			using (WebClient wc = new WebClient())
			{
				//보안 프로토콜 버전. 기본 연결이 닫혔습니다. 예기치 않게 연결이 닫혔습니다. 오류 해결을 위해 넣는 코드.
				ServicePointManager.SecurityProtocol |= SecurityProtocolType.Ssl3;
				ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls;
				ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11;
				ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
								

				//sw1서버에서는 아래 경로 접근가능함. 내PC에서는 안됨.
				//http://121.189.59.98:8095/pibo0701.asp?ServiceID=pibo0701&phone=01074703691 
				//호출결과값화면
				//ReturnCode=ok&UUID=61a59f76-6cdf-4fc0-8542-1fc30279583b&Message=

				string callURL = "http://121.189.59.98:8095/pibo0701.asp?ServiceID=pibo0701&phone=" + phno;
				var downloadData = new WebClient().DownloadData(callURL);
				//var downloadString = System.Text.Encoding.UTF8.GetString(downloadData);   //UTF8로 갖고올때.
				var downloadString = System.Text.Encoding.Default.GetString(downloadData);  //대상경로가 euckr인데 UTF8로 갖고오면 한글깨짐. 이 때 .Default로 갖고오면 한글 안깨짐.
				JObject loginAPIData = JObject.Parse(downloadString);
				//JArray result = (JArray)loginAPIData["result"];
				string result = (string)loginAPIData["result"];
				string g_returnCode = (string)loginAPIData["ReturnCode"];
				string g_uuid = (string)loginAPIData["UUID"];

				if (g_returnCode.Equals("noapp"))
				{
					result_descript = "주식창 설치 후 로그인 가능합니다. 주식창 설치 문자를 확인하세요.";
					
					ViewBag.tmp_g_returnCode = "";
					ViewBag.tmp_g_uuid = "";

					return Content("<script type=\"text/javascript\">alert(\""+ result_descript+ "\"); window.close();</script>");
				}
				else if (g_returnCode.Equals("error") && g_uuid.Equals("Message=PLEASE TURN ON PUSH ALARM!"))
				{
					result_descript = "주식창 설치 후 로그인 가능합니다. 주식창 설치 문자를 확인하세요.";

					ViewBag.tmp_g_returnCode = "";
					ViewBag.tmp_g_uuid = "";

					return Content("<script type=\"text/javascript\">alert(\"" + result_descript + "\"); window.close();</script>");
				}
				else
				{
					ViewBag.tmp_g_returnCode = g_returnCode;
					ViewBag.tmp_g_uuid = g_uuid;
					
					//쿠키 생성
					Response.Cookies["mobile"].Value = phno;
					Response.Cookies["devk"].Value = g_uuid;
					Response.Cookies["rstat"].Value = "Y";
					Response.Cookies["rcnt"].Value = "0";

					return Redirect("/");

					//return Redirect("https://login.wowtv.co.kr/sso/logout?ReturnUrl=" + System.Configuration.ConfigurationManager.AppSettings["DomainUrlFront"]);
				}
			}

			return View();
		}
		

		// GET: Home
		public ActionResult Index()
		{
			//위닉스 API 무료 생방송 중에 가장 먼저 만들어진 1개만 던져줌
			string result;
			string title;
			string channel;

			using (WebClient wc = new WebClient())
			{
				var downloadData = new WebClient().DownloadData("http://121.189.59.98:8095/pico1090.asp?ServiceID=pico1090");
				//var downloadString = System.Text.Encoding.UTF8.GetString(downloadData);   //UTF8로 갖고올때.
				var downloadString = System.Text.Encoding.Default.GetString(downloadData);  //대상경로가 euckr인데 UTF8로 갖고오면 한글깨짐. 이 때 .Default로 갖고오면 한글 안깨짐.
				JObject chatAPIData = JObject.Parse(downloadString);
				//JArray result = (JArray)chatAPIData["result"];
				result = (string)chatAPIData["result"];
				title = (string)chatAPIData["title"];
				channel = (string)chatAPIData["channel"];
				string url = (string)chatAPIData["url"];

				if (!channel.Equals(""))
				{
					ViewBag.tmp_title = title;
					ViewBag.tmp_channel = channel;
					ViewBag.tmp_url = url;
				}
				else
				{
					ViewBag.tmp_title = "";
					ViewBag.tmp_channel = "";
					ViewBag.tmp_url = "";
				}
			}
			
			var resultData = client.MainLivePartner(channel);	//위닉스API로 받아온 channel값 넣어서 해당 파트너 관련 글 가져오기.

			return View(resultData);





			//ViewBag.Message = client.Message();

			//int x = client.add(5, 7);
			//ViewBag.X = 5;
			//ViewBag.Y = 7;
			//ViewBag.Z = x;

			//return View();
		}

		//bcode로 TAB_SUMNAIL 데이터 불러오기.
		public ActionResult findByBcode(string bcode)
		{
			var resultData = client.findByBcode(bcode);

			return View(resultData);
		}

		//topmenu의 경우 topMenuList() 액션 쓰지않고, _SwLayOut.cshtml파일에서 직접 topmenu 데이터를 호출해서 씀.
		public ActionResult topMenuList()
		{
			var resultData = client.topMenuList();
			ViewBag.TotalCount = resultData.Count();

			return View(resultData);
		}

		//위닉스API에서 보내준 파트너명 받아서, 해당 파트너 관련 데이터 1열 갖고오기
		public ActionResult MainLivePartner(string channel)
		{
			var resultData = client.MainLivePartner(channel);

			return View(resultData);
		}

		//메인상단바
		public ActionResult sw_mainTopBar(int seq)
		{
			var resultData = client.sw_mainTopBar(seq);

			return View(resultData);
		}




		public ActionResult KvinaNoticeView(int seq)
		{
			//뷰
			KvinaServiceClient KvinaBoard = new KvinaServiceClient();

			var resultData = KvinaBoard.KvinaNoticeView(seq);

			return View(resultData);
		}









		private string GetAuthHeaderData(string strMethod)
		{
			///-------------------------
			/// 변수 초기화
			///-------------------------
			string pl_strRequestBodyBase64String = string.Empty;                                   // RequestBody (Base64 - Encode)
			string pl_strGenerateAuthTokens = string.Empty;                                   // Authorization 요청 헤더의 최종 데이터
			string pl_AppId = "5c171ab597884d6ba8f563f45db44772";							// APP ID (테스트 / 운영 환경별로 다름)
			string pl_AppKey = "4rG7cgdj/a21/6NOvvqxuT0ltuoUfs5PwEvesL/+vaw=";				// APP Key (테스트 / 운영 환경별로 다름)
			string pl_strRequestMethod = string.Empty;                                   // Request Method (API Method에 따라 달라질 수 있음)

			///------------------------------------------
			/// 1. Signature 생성을 위한 데이터 설정
			///------------------------------------------

			///--------------------------------
			/// 1-1. Request Method 대문자 처리
			///--------------------------------
			pl_strRequestMethod = strMethod.ToUpper();
			Response.Write(string.Format("pl_strRequestMethod = {0}", pl_strRequestMethod) + "<br/>");

			///-------------------------
			/// 1-2. UNIX timestamp 획득
			///-------------------------
			DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan timeSpan = DateTime.UtcNow - epochStart;
			string pl_strRequestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();
			//pl_strRequestTimeStamp = "1583391560";
			Response.Write(string.Format("pl_strRequestTimeStamp = {0}", pl_strRequestTimeStamp) + "<br/>");

			///--------------------------------------------------
			/// 1-3. Nonce 설정 (클라이언트 생성 임의 데이터)
			///--------------------------------------------------
			//string pl_strNonce = string.Format("{0}{1}", DateTime.Today.ToString("yyyyMMdd"), (int)((new Random()).NextDouble() * 10000000));
			//pl_strNonce = "20200305571158";

			//Response.Write(string.Format("pl_strNonce = {0}", pl_strNonce) + "<br/>");


			var pl_strNonce = System.Guid.NewGuid();
			Response.Write(string.Format("pl_strNonce = {0}", pl_strNonce) + "<br/>");

			///--------------------------------
			/// 1-4. APP Key Decode(Base64)
			///--------------------------------
			byte[] pl_arrSecretKeyByteArray = Convert.FromBase64String(pl_AppKey);

			///-----------------------------------------------
			/// 1-5. Signature 생성을 위한 데이터 조합
			///-----------------------------------------------
			string pl_strSignatureRawData = string.Format("{0}{1}{2}{3}", pl_AppId, pl_strRequestMethod, pl_strRequestTimeStamp, pl_strNonce);
			Response.Write(string.Format("pl_strSignatureRawData = {0}", pl_strSignatureRawData) + "<br/>");

			///-----------------------------------------------
			/// 1-6. Signature 생성
			///   1) HMAC-SHA256 Hash 생성
			///   2) 생성된 Hash 데이터 인코딩(Base64)
			///   3) 최종 Signature 생성 완료
			///-----------------------------------------------
			byte[] pl_arrSignature = Encoding.UTF8.GetBytes(pl_strSignatureRawData);
			using (HMACSHA256 hmac = new HMACSHA256(pl_arrSecretKeyByteArray))
			{
				byte[] pl_arrSignatureBytes = hmac.ComputeHash(pl_arrSignature);
				string pl_strRequestSignatureBase64String = Convert.ToBase64String(pl_arrSignatureBytes);
				Response.Write(string.Format("pl_strRequestSignatureBase64String = {0}", pl_strRequestSignatureBase64String) + "<br/>");

				pl_strGenerateAuthTokens = string.Format("{0} {1}:{2}:{3}:{4}", "PLTOKEN", pl_AppId, pl_strRequestSignatureBase64String, pl_strNonce, pl_strRequestTimeStamp);
			}

			return pl_strGenerateAuthTokens;
		}

		public ActionResult Api_ProductList(string method)
		{
			if (method == null)
			{
				method = "get";
			}
			var access_token = GetAuthHeaderData(method);		
			

			using (WebClient wc = new WebClient())
			{
				wc.Headers.Add("Authorization", access_token);

				string callURL = "https://swbillapi.wowtv.co.kr/v1/payment/offline/productlist";
				var downloadData = new WebClient().DownloadData(callURL);
				//var downloadString = System.Text.Encoding.UTF8.GetString(downloadData);   //UTF8로 갖고올때.
				var downloadString = System.Text.Encoding.Default.GetString(downloadData);  //대상경로가 euckr인데 UTF8로 갖고오면 한글깨짐. 이 때 .Default로 갖고오면 한글 안깨짐.
				JObject returnData = JObject.Parse(downloadString);
				string resultCode = (string)returnData["resultCode"];
				JArray data = (JArray)returnData["data"];
				string orderNo = (string)returnData["orderNo"];
				string totalCnt = (string)returnData["totalCnt"];

				ViewBag.resultCode = resultCode;
				ViewBag.orderNo = orderNo;
				ViewBag.totalCnt = totalCnt;

			}

			return View();
		}


		public ActionResult HttpClient_Api_ProductList(string method)
		{
			if (method == null)
			{
				method = "get";
			}
			string access_token = GetAuthHeaderData(method);

			using (HttpClient hc = new HttpClient())
			{

				
				//hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(access_token);

				hc.DefaultRequestHeaders.Add("Authorization", access_token);

				string callURL = "https://swbillapi.wowtv.co.kr/v1/payment/offline/productlist";
				var downloadData = new WebClient().DownloadData(callURL);
				//var downloadString = System.Text.Encoding.UTF8.GetString(downloadData);   //UTF8로 갖고올때.
				var downloadString = System.Text.Encoding.Default.GetString(downloadData);  //대상경로가 euckr인데 UTF8로 갖고오면 한글깨짐. 이 때 .Default로 갖고오면 한글 안깨짐.
				JObject returnData = JObject.Parse(downloadString);
				string resultCode = (string)returnData["resultCode"];
				JArray data = (JArray)returnData["data"];
				string orderNo = (string)returnData["orderNo"];
				string totalCnt = (string)returnData["totalCnt"];

				ViewBag.resultCode = resultCode;
				ViewBag.orderNo = orderNo;
				ViewBag.totalCnt = totalCnt;
			}

			return View();
		}



		public ActionResult HttpWebRequest_Api_ProductList(string method)
		{
			if (method == null)
			{
				method = "get";
			}
			var access_token = GetAuthHeaderData(method);

			var upper_method = method.ToUpper();	//대문자로 표기.

			string url = "https://swbillapi.wowtv.co.kr/v1/payment/offline/productlist";
			HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
			//hwr.Method = "POST";
			hwr.Method = upper_method;
			hwr.Headers.Add("Accept-Language", "UTF-8");
			hwr.Headers.Add("Authorization", access_token);


			string contents = "";
			bool isSuccess = false;
			string returnMessage = "";
			HttpStatusCode statusCode = HttpStatusCode.NotImplemented;
			string statusDescription = "";

			string data = "propertyKeys=[\"id\",\"name\",\"kaccount_email\"]";
			byte[] bytes = Encoding.ASCII.GetBytes(data);
			using (Stream reqStream = hwr.GetRequestStream())
			{
				reqStream.Write(bytes, 0, bytes.Length);
			}

			using (HttpWebResponse res = (HttpWebResponse)hwr.GetResponse())
			{
				statusCode = ((HttpWebResponse)res).StatusCode;
				statusDescription = ((HttpWebResponse)res).StatusDescription;
				if (statusCode == HttpStatusCode.OK)
				{
					Stream dataStream = res.GetResponseStream();
					StreamReader reader = new StreamReader(dataStream, System.Text.Encoding.GetEncoding("UTF-8"), true);
					contents = reader.ReadToEnd();
					isSuccess = true;
				}
				else
				{
					returnMessage = statusDescription;
				}
			}

			return Json(new
			{
				IsSuccess = isSuccess,
				ReturnMessage = ""
			}, JsonRequestBehavior.AllowGet);
			
		}


		public String HttpCall(String p_sParam, String p_sMethod)
		{
			try
			{
				HttpWebRequest httpWebRequest = null;
				// 인코딩 UTF-8
				byte[] sendData = UTF8Encoding.UTF8.GetBytes(p_sParam);


				var access_token = GetAuthHeaderData(p_sMethod);

				string url = "https://swbillapi.wowtv.co.kr/v1/payment/offline/productlist";


				if (p_sMethod == "POST")
				{
					httpWebRequest = (HttpWebRequest)WebRequest.Create(url);					
					httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
					httpWebRequest.Method = p_sMethod;
					httpWebRequest.Headers.Add("Authorization", access_token);
					httpWebRequest.ContentLength = sendData.Length;

					Stream requestStream = httpWebRequest.GetRequestStream();
					requestStream.Write(sendData, 0, sendData.Length);
					requestStream.Close();
				}
				else if (p_sMethod == "GET")
				{
					httpWebRequest = (HttpWebRequest)WebRequest.Create(url + " ? " + p_sParam);
					httpWebRequest.Method = p_sMethod;
					httpWebRequest.Headers.Add("Authorization", access_token);
				}

				HttpWebResponse httpWebResponse;
				using (httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
					string result = streamReader.ReadToEnd();
					return result;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("error: " + e);
				return String.Empty;
			}
		}





		/*
		public partial class Test_WebRequest : System.Web.UI.Page
		{
		*/
			public void Page_Load(object sender, EventArgs e)
			{

				var access_token = GetAuthHeaderData("GET");	//인증토큰값 -> "PLTOKEN 5c171ab597884d6ba8f563f45db44772:WOwmC+5tg01Gqv946GN4gs5uGM1STgbfC5yXOk9qtzk=:95d1a03c-6f3d-476f-8583-e01dab506864:1605256473"

				string strResponse;
				string strErrMsg;
				//GetXMLHttp("{}", "https://swbillapi.wowtv.co.kr/v1/payment/offline/productlist", "get", HttpContentType.Json, "Authorization", "PLTOKEN 5c171ab597884d6ba8f563f45db44772:6+8AXUmf6wUDI6/TFa60wr0eUs0/WU3gJRdOmOy5zCc=:48d4da111d664a63b07a2aaeaed1f151:1605251295", out strResponse, out strErrMsg);
				GetXMLHttp("{}", "https://swbillapi.wowtv.co.kr/v1/payment/offline/productlist", "get", HttpContentType.Json, "Authorization", access_token, out strResponse, out strErrMsg);
				Response.Write("<BR>strErrMsg=" + strErrMsg + " < br>");
				Response.Write("<BR>strResponse=" + strResponse);
			}

			public int GetXMLHttp(string strRequest, string strUrl, string strMethod, HttpContentType objContentType, string strHeaderKey
										, string strHeaderValue, out string strResponse, out string strErrMsg)
			{
				int pl_intRetVal = 0;
				HttpWebRequest pl_objRequest = null;
				HttpWebResponse pl_objResponse = null;
				StreamReader pl_objStreamReader = null;
				Stream pl_objReqStream = null;

				strResponse = string.Empty;
				strErrMsg = string.Empty;

				try
				{
					// Setting Request
					if (strMethod.ToLower().Equals("get"))
					{
						pl_objRequest = (HttpWebRequest)HttpWebRequest.Create(strUrl + "?" + strRequest);
						pl_objRequest.Method = WebRequestMethods.Http.Get;
						if (!string.IsNullOrEmpty(strHeaderKey))
						{
							pl_objRequest.Headers.Add(strHeaderKey, strHeaderValue);
						}
					}
					else
					{
						byte[] pl_bytes = Encoding.UTF8.GetBytes(strRequest);

						pl_objRequest = (HttpWebRequest)HttpWebRequest.Create(strUrl);
						pl_objRequest.KeepAlive = false;
						pl_objRequest.Method = WebRequestMethods.Http.Post;
						pl_objRequest.ContentLength = pl_bytes.Length;
						pl_objRequest.ServicePoint.Expect100Continue = false;
						if (!string.IsNullOrEmpty(strHeaderKey))
						{
							pl_objRequest.Headers.Add(strHeaderKey, strHeaderValue);
						}

						switch (objContentType)
						{
							case HttpContentType.Html:
								pl_objRequest.ContentType = "text/html";
								break;
							case HttpContentType.Json:
								pl_objRequest.ContentType = "application/json";
								break;
							case HttpContentType.Xml:
								pl_objRequest.ContentType = "application/xml";
								break;
							case HttpContentType.Plain:
								pl_objRequest.ContentType = "text/plain";
								break;
							default:
								pl_objRequest.ContentType = "application/x-www-form-urlencoded";
								break;
						}

						pl_objReqStream = pl_objRequest.GetRequestStream();
						pl_objReqStream.Write(pl_bytes, 0, pl_bytes.Length);
						pl_objReqStream.Close();
					}

					pl_objRequest.Timeout = 60000;

					// Get Response
					pl_objResponse = (HttpWebResponse)pl_objRequest.GetResponse();

					// Check Status
					int pl_intHttpStatus = pl_objResponse.StatusCode.GetHashCode();
					if (pl_intHttpStatus != (int)HttpStatusCode.OK)
					{
						pl_intRetVal = pl_intHttpStatus;
						strErrMsg = pl_objResponse.StatusDescription;
					}
					else
					{
						pl_intRetVal = 0;
						strErrMsg = string.Empty;
						pl_objStreamReader = new StreamReader(pl_objResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
						strResponse = pl_objStreamReader.ReadToEnd();
						pl_objStreamReader.Close();
					}
					pl_objResponse.Close();
				}
				catch (WebException pl_objEx)
				{
					strErrMsg = pl_objEx.Message;

					if (pl_objEx.Status == WebExceptionStatus.ProtocolError)
					{
						using (Stream data = pl_objEx.Response.GetResponseStream())
						using (var reader = new StreamReader(data))
						{
							strResponse = reader.ReadToEnd();
						}
					}
				}
				finally
				{
					pl_objRequest = null;
					pl_objResponse = null;
					pl_objStreamReader = null;
					pl_objReqStream = null;
				}
				return pl_intRetVal;
			}

			public enum HttpContentType
			{
				Html = 1,
				Json = 2,
				Xml = 3,
				Plain = 4,
				Others = 99
			}
		/*
		}
		*/






	}

}
