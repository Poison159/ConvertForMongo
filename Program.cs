using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LocationMapperApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // var oldEndpoint = "https://zkhiphava.co.za/api/Indawoes?userLocation=27.8552855,-26.2667783&distance=100&vibe=pub/bar&city=Gauteng";
            var newEndpoint = "http://localhost:5230/Locations";
            var baseUrl = "https://zkhiphava.co.za/api/Indawoes";
            var httpClient = new HttpClient();
            var locations = await getAllOldLocations(baseUrl);
            // Define conversion functions
           

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Indawo, AddLocationRequestObject>()
                    .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.images.Select(i => i.imgPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last())))
                    .ForMember(dest => dest.OperatingHours, opt => opt.MapFrom(src => src.oparatingHours.Select(oh => new OperatingHourTwo
                    {
                        Day = ParseDayOfWeek(oh.day),
                        Occasion = oh.occation,
                        OpeningHour = ParseTimeOnly(oh.openingHour),
                        ClosingHour = ParseTimeOnly(oh.closingHour)
                    })))
                    .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.instaHandle))
                    .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.city))
                    .ForMember(dest => dest.SpecialInstructions, opt => opt.MapFrom(src => src.specialInstructions.Select(si => si.instruction)));
            });

            var mapper = config.CreateMapper();

            foreach (var location in locations)
            {
                var addLocationRequest = mapper.Map<AddLocationRequestObject>(location);
                var json = JsonConvert.SerializeObject(addLocationRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(newEndpoint, content);
                Console.WriteLine($"Status: {result.StatusCode}");
            }
        }

        static DayOfWeek ParseDayOfWeek(string day) => Enum.Parse<DayOfWeek>(day, true);

        static TimeOnly ParseTimeOnly(DateTime dateTime) => new TimeOnly(dateTime.Hour, dateTime.Minute);

        public static async Task<List<Indawo>> getAllOldLocations(string baseUrl)
        {
            var userLocation = "27.8552855,-26.2667783";
            var distance = 10000;
            var vibes = new List<string> { "pub/bar", "club", "chilled", "outdoor" };
            var provinces = new List<string> { "Gauteng", "Western Cape", "KwaZulu-Natal" };
            var httpClient = new HttpClient();
            var combinedLocations = new List<Indawo>();

            foreach (var vibe in vibes)
            {
                foreach (var province in provinces)
                {
                    var endpoint = $"{baseUrl}?userLocation={userLocation}&distance={distance}&vibe={vibe}&city={province}";
                    var response = await httpClient.GetStringAsync(endpoint);
                    var locations = JsonConvert.DeserializeObject<List<Indawo>>(response);
                    combinedLocations.AddRange(locations!);
                }
            }
            return combinedLocations;
        }
    }



    public class Indawo
    {
        public Indawo()
        {
            images = new List<Image>();
            operatingHoursStr = new List<string>();
            imgPath = "~/Content/user.png";
            open = true;
            closingSoon = false;
        }

        public int id { get; set; }


        public string type { get; set; }


        public string city { get; set; }


        public double rating { get; set; }


        public double entranceFee { get; set; }


        public string name { get; set; }


        public string lat { get; set; }

        public string lon { get; set; }


        public string address { get; set; }


        public string imgPath { get; set; }

        public string instaHandle { get; set; }

        public OperatingHours[] oparatingHours { get; set; }

        public List<SpecialInstruction> specialInstructions { get; set; }

        public List<Image> images { get; set; }

        public double distance { get; set; }

        public string info { get; set; }

        public string openOrClosedInfo { get; set; }

        public List<string> operatingHoursStr { get; set; }

        public bool open { get; set; }

        public bool closingSoon { get; set; }

        public bool openingSoon { get; set; }
    }

    public class OperatingHours
    {
        public int id { get; set; }

        public int indawoId { get; set; }

        public string day { get; set; }
        public string occation { get; set; }

        public DateTime openingHour { get; set; }

        public DateTime closingHour { get; set; }
    }

    public class SpecialInstruction
    {
        public int id { get; set; }
        public int indawoId { get; set; }
        public string instruction { get; set; }
    }

    public class Image
    {
        public int id { get; set; }

        public int indawoId { get; set; }
        public string eventName { get; set; }

        public string imgPath { get; set; }
    }

    // --------------------------------------- NEW MODELS ---------------------------------------

    public class AddLocationRequestObject
    {
        /// <summary>
        /// Gets or sets the name of entity.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of entity.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the rating of entity.
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the image path of the entity.
        /// </summary>
        public string ImgPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the province of the entity.
        /// </summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL of the entity.
        /// </summary>
        public string Url { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the description of the entity.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
		/// Gets or sets the list of operating hours for the location.
		/// </summary>
        public List<OperatingHourTwo> OperatingHours { get; set; } = new List<OperatingHourTwo>();


        /// <summary>
        /// Gets or sets the list of images for the location.
        /// </summary>
        public List<string> Images { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of special instructions for the location.
        /// </summary>
        public List<string> SpecialInstructions { get; set; } = new List<string>();

    }

    public class OperatingHourTwo
    {
        /// <summary>
        /// Gets or sets the day of the week for the operating hour.
        /// </summary>

        public DayOfWeek? Day { get; set; }

        /// <summary>
        /// Gets or sets the occasion for the operating hour.
        /// </summary>

        public string? Occasion { get; set; }

        /// <summary>
        /// Gets or sets the opening hour of the operating hour.
        /// </summary>

        public TimeOnly OpeningHour { get; set; }

        /// <summary>
        /// Gets or sets the closing hour of the operating hour.
        /// </summary>

        public TimeOnly ClosingHour { get; set; }
    }

    // Define other models like OperatingHour, AddLocationRequestObject as provided in the question
}