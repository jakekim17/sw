using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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



	}
}