using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;
using System.Text;
using System.Web;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebApiController : ControllerBase
    {
        //브라우저에서 전달받은 Instnace ID Token 저장
        [Route("~/api/save-token"), HttpPost]
        //[Route("save-token"), HttpPost]
        public async Task<ActionResult> SaveSubscription()
        {
            try
            {
                //1. Instnace ID Token 조회
                string pPostBody = "";

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    pPostBody = await reader.ReadToEndAsync();
                }

                string[] lPostBody = pPostBody.Split('|');

                string lBodyContent = HttpUtility.UrlEncode("앱 푸쉬 내용입니다.");
                Console.WriteLine(lBodyContent);
                // 푸쉬 전송
                await sendMessage(lPostBody[1], lBodyContent, lPostBody[0]);


                //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                //return response;
                return Ok();
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false, Error = error });
                //return response;
                return Problem(error);
            }
        }



        //Google FCM 서버에 Push Message 발송요청
        public async Task sendMessage(string title, string msg, string instanceIdToken)
        {
            //1. 전송정보 생성
            string serverKey = "key=AAAAOUKni2g:APA91bFducLWEwYQHwPcLFGXit-GX6P5bIDmCqLqZp7RYL3EaLFv8kl8w9Hz2w5Q8supUykxfUZZzdhgZfVvY1nMvZy3BaWrt5DXTfNw5eyK2ymsoySfBDriZR4mdYdZKO7rIt__a9TT";
            string url = "https://fcm.googleapis.com/fcm/send";
            string postJson = (@"{
                            'content_available': false,
                            'data':
                            {
                                'title': '" + title + @"',
                                'body': '" + msg + @"'

                            },
                            'to': '" + instanceIdToken + @"'
                        }")
                                .Replace("'", "\"")
                                .Trim();

            //2. Firebase 서버에 REST 전송
            using (HttpClient client = new HttpClient() { BaseAddress = new Uri(url) })
            {
                //1) 요청
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", serverKey);
                StringContent postEncoded = new StringContent(postJson.Replace("\t", "").Replace("\r\n", ""), Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = await client.PostAsync("", postEncoded);

                //2) 응답분석
                string responseText = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode)
                    Console.WriteLine(responseText);
                    //Debug.WriteLine("전송성공", responseText);
                else
                    Console.WriteLine(responseText);
                    //Debug.WriteLine("전송실패:", responseText, httpResponse.StatusCode);
            }
        }

    }
}
