import { NextResponse } from "next/server";

export async function GET() {
  try {
    // Open-Meteo Marine API for Bodrum (Latitude: 37.0344, Longitude: 27.4305)
    const url = `https://marine-api.open-meteo.com/v1/marine?latitude=37.0344&longitude=27.4305&current=wave_height,wave_direction,wave_period&timezone=auto`;
    const weatherUrl = `https://api.open-meteo.com/v1/forecast?latitude=37.0344&longitude=27.4305&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,cloud_cover,pressure_msl,surface_pressure,wind_speed_10m,wind_direction_10m,wind_gusts_10m&timezone=auto`;

    const [marineRes, weatherRes] = await Promise.all([
      fetch(url),
      fetch(weatherUrl)
    ]);

    const marineData = await marineRes.json();
    const weatherData = await weatherRes.json();

    return NextResponse.json({
      location: "Bodrum",
      current: {
        temp: weatherData.current.temperature_2m,
        windSpeed: weatherData.current.wind_speed_10m,
        windDirection: weatherData.current.wind_direction_10m,
        waveHeight: marineData.current.wave_height,
        weatherCode: weatherData.current.weather_code,
        isDay: weatherData.current.is_day
      },
      unit: {
        temp: "°C",
        wind: "km/h",
        wave: "m"
      }
    });
  } catch (error) {
    console.error("Weather fetch error:", error);
    return NextResponse.json({ error: "Failed to fetch weather data" }, { status: 500 });
  }
}
