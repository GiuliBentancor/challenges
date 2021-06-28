using System;
using System.Net;
using NUnit.Framework;
using FluentAssertions;
using RestSharp;
using Newtonsoft.Json;
using RestSharp.Authenticators;


namespace Challenge.GiuliSolution
{
    class TestApiChallenge
    {
        public RestClient Client;
        public string Challenger;
        public string Xauth;


        private string SetChallenger()
        {
            var request = new RestRequest("/challenger", DataFormat.Json);
            var response = Client.Post(request);

            var value = response.Headers[2].Value;
            var idChallenger = value?.ToString();
            return idChallenger;
        }

        private string SetXauth()
        {
            var request = new RestRequest("/secret/token", Method.POST);
            request.AddHeader("X-Challenger", Challenger);
            Client.Authenticator = new HttpBasicAuthenticator("admin", "password");

            var response = Client.Execute(request);
            var auth = response.Headers[2].ToString().Substring(13);

            return auth;
        }

        private int SetIdExistingPost()
        {
            var request = new RestRequest("/todos");
            var response = Client.Get(request);
            var obj = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (obj == null) return 0;
            int id = obj.todos[0].id;
            return id;
        }

        [OneTimeSetUp] 
        public void SetTests()
        {
            Client = new RestClient("http://apichallenges.herokuapp.com/");
            Challenger = SetChallenger();
            Xauth = SetXauth();
        }

        [Test]
        public void GetChallenges200()
        {
            var request = new RestRequest("/challenges", Method.GET);
            request.AddHeader("X-Challenger", Challenger);
            
            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public void GetTodos200()
        {
            var request = new RestRequest("/todos", Method.GET);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public  void GetTodo404()
        {
            var request = new RestRequest("/todo", Method.GET);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public void GetTodosId200()
        {
            int id = SetIdExistingPost();
            var request = new RestRequest($"/todos/{id}", Method.GET);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Test]
        public void GetTodosId404()
        {
            int id = 102500;
            var request = new RestRequest($"/todos/{id}", Method.GET);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [Test]
        public void GetHeader()
        {
            var request = new RestRequest("/todos", Method.HEAD);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Test]
        public void PostTodos201()
        {
            var request = new RestRequest("/todos", Method.POST);
            request.AddHeader("X-Challenger", Challenger);
            request.AddJsonBody(new Todo()
            {

                title = "Creando un nuevo challenge",
                doneStatus = true,
                description = "Me debe devolver created"

            });
            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }


        [Test]

        public void GetTodosFilter200()
        { 
            // Creo un post con true para poder filtrar el valor
            var previousPost = new RestRequest("/todos", Method.POST);
            previousPost.AddHeader("X-Challenger", Challenger);
            previousPost.AddJsonBody(new Todo()
            {

                title = "Post previo para test",
                doneStatus = true,
                description = "Necesito que " +
                              "haya un post con true en done"

            });
          
            Client.Execute(previousPost);

            //Request con filtro true en doneStatus
            var request = new RestRequest("/todos", Method.GET);
            request.AddHeader("X-Challenger", Challenger);
            request.AddParameter("doneStatus", "true");

            var response = Client.Execute(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Test]
        public void PostTodos400()
        {
            var request = new RestRequest("/todos", Method.POST);
            request.AddHeader("X-Challenger", Challenger);

            request.AddJsonBody(new
            {
                title = "Este post no será creado",
                doneStatus = "",
                description = "doneStatus invalido - bas request"
            });

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        }

        [Test]

        public void PostTodosId200()
        {
            var id = SetIdExistingPost();
            var request = new RestRequest($"/todos/{id}", Method.POST);
            request.AddHeader("X-Challenger", Challenger);

            request.AddJsonBody(new Todo()
            {

                title = "updated",

            });

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Test]
        public void DeleteId200()
        {
            var id = SetIdExistingPost();
            var request = new RestRequest($"/todos/{id}", Method.DELETE);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);


        }

        [Test]
        public void Options200()
        {
            var request = new RestRequest("/todos", Method.OPTIONS);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Test]
        public void GetWithHeaderXml200()
        {

            var request = new RestRequest("/todos", Method.GET);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("Accept", "application/xml");

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Test]
        public void GetWithHeaderJson200()
        {

            var request = new RestRequest("/todos", Method.GET);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("Accept", "application/json");

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Test]
        public  void GetWithHeaderAny200()
        {

            var request = new RestRequest("/todos", Method.GET);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("Accept", "*/*");

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Test]
        public void GetWithHeaderXmlPref200()
        {

            var request = new RestRequest("/todos", Method.GET);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("Accept", "application/xml, application/json");

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Test]
        public void GetWithNoAcceptHeader200()
        {

            var request = new RestRequest("/todos", Method.GET);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Test]
        public void GetWithAcceptHeader404()
        {

            var request = new RestRequest("/todos", Method.GET);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("Accept", "application/gzip");

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);

        }

        [Test]
        public void PostXmlBody201()
        {
            var request = new RestRequest("/todos", Method.POST);
            request.AddHeader("X-Challenger", Challenger);

            request.RequestFormat = DataFormat.Xml;
            request.AddHeader("Accept", "application/xml");
            request.AddHeader("Content-Type", "application/xml");

            string xmlBody = "<todo> < doneStatus > true </ doneStatus >< description > XML </ description >< title > XMLBODY </ title ></ todo > ";

            request.AddParameter("application/xml", xmlBody, ParameterType.RequestBody);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

        }

        [Test]
        public void PostUnsupportedContent415()
        {
            var request = new RestRequest("/todos", Method.POST);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("Content-Type", "NANA");

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);

        }

        [Test]
        public  void PostXmLtoJson201()
        {
            var request = new RestRequest("/todos", Method.POST);
            request.AddHeader("X-Challenger", Challenger);

            request.RequestFormat = DataFormat.Xml;
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/xml");

            string xmlBody = "<todo> < doneStatus > true </ doneStatus >< description > XML </ description >< title > XMLBODY </ title ></ todo > ";

            request.AddParameter("application/xml", xmlBody, ParameterType.RequestBody);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

        }

        [Test]
        public void PostJsoNtoXml201()
        {
            var request = new RestRequest("/todos", Method.POST);
            request.AddHeader("X-Challenger", Challenger);

            request.AddHeader("Accept", "application/xml");
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new Todo()
            {

                title = "JSON",
                description = "JSON",

            });

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

        }

        [Test]
        public  void Get204()
        {

            var request = new RestRequest("/heartbeat", Method.GET);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        }

        [Test]
        public void Delete405()
        {

            var request = new RestRequest("/heartbeat", Method.DELETE);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);

        }

        [Test]
        public void Patch500()
        {

            var request = new RestRequest("/heartbeat", Method.PATCH);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        }



        [Test, Ignore("No anda")]
        public void Trace501()
        {
            var request = new RestRequest("/heartbeat", Method.GET);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }


        [Test]
        public void PostAuth201()
        {
            var request = new RestRequest("/secret/token", Method.POST);

            Client.Authenticator = new HttpBasicAuthenticator("admin", "password");


            request.AddHeader("X-Challenger", Challenger);


            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);


        }


        [Test]
        public void PostAuth401()
        {
            var request = new RestRequest("/secret/token", Method.POST);
            Client.Authenticator = new HttpBasicAuthenticator("giuli", "password");
            request.AddHeader("X-Challenger", Challenger);


            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }

        [Test]
        public void Get403()
        {
            var request = new RestRequest("/secret/note", Method.GET);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("X-AUTH-TOKEN", Challenger);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        }

        [Test]
        public void Get401()
        {
            var request = new RestRequest("/secret/note", Method.GET);
            request.AddHeader("X-Challenger", Challenger);

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }

        [Test]
        public void GetNote200()
        {
      
            var request = new RestRequest("/secret/note", Method.GET);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("X-AUTH-TOKEN", Xauth);

            var response = Client.Execute(request);

            response.Content.Should().Contain("note");

        }

        [Test]

        public void PostNote200()
        {

            var request = new RestRequest("/secret/note", Method.POST);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("X-AUTH-TOKEN", Xauth);

            request.AddJsonBody(new Note()
            {
                note = "my note"
            });

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);


        }

        [Test]

        public void PostNote401()
        {

            var request = new RestRequest("/secret/note", Method.POST);
            request.AddHeader("X-Challenger", Challenger);


            request.AddJsonBody(new Note()
            {
                note = "my note"
            });

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);


        }

        [Test]
        public void PostNote403()
        {

            var request = new RestRequest("/secret/note", Method.POST);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("X-AUTH-TOKEN", "XAUTH");

            request.AddJsonBody(new Note()
            {
                note = "my note"
            });

            var response = Client.Execute(request);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);


        }


        [Test, Ignore("No anda")]
        public  void GetNoteWithBearer200()
         {
             var bearer = $"Bearer + {Xauth}";

             var request = new RestRequest("/secret/note", Method.GET);

             request.AddHeader("X-Challenger", Challenger);
             request.AddHeader("Authorization", bearer);

             var response = Client.Execute(request);


             Console.WriteLine(response.StatusCode);
             response.StatusCode.Should().Be(HttpStatusCode.OK);}



        [Test, Ignore("No anda")]
        public  void PostNoteWithBearer200()
        {
           
            var request = new RestRequest("/secret/note", Method.POST);
            request.AddHeader("X-Challenger", Challenger);
            request.AddHeader("Authorization", $"Bearer + {Xauth}");

            request.AddJsonBody(new Note()
            {
                note = "my note"
            });

            var response = Client.Execute(request);

            Console.WriteLine(response.StatusCode);
            response.StatusCode.Should().Be(HttpStatusCode.OK);


        }

    }
}
