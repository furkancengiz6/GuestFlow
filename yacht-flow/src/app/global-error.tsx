"use client";

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <html>
      <body style={{ 
        backgroundColor: "#050608", 
        color: "#ffffff", 
        fontFamily: "system-ui, -apple-system, sans-serif",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100vh",
        margin: 0,
        padding: "2rem"
      }}>
        <div style={{ textAlign: "center", maxWidth: "500px" }}>
          <div style={{ fontSize: "4rem", marginBottom: "1.5rem" }}>⚓</div>
          <h2 style={{ 
            fontSize: "2rem", 
            fontWeight: 300, 
            marginBottom: "1rem",
            color: "#c9a54e"
          }}>
            Temporary Navigation Issue
          </h2>
          <p style={{ 
            color: "rgba(255,255,255,0.5)", 
            lineHeight: 1.8,
            marginBottom: "2rem" 
          }}>
            Our systems encountered an unexpected wave. Please try again.
          </p>
          <button
            onClick={() => reset()}
            style={{
              backgroundColor: "#c9a54e",
              color: "#050608",
              border: "none",
              padding: "1rem 3rem",
              borderRadius: "2rem",
              fontSize: "0.7rem",
              letterSpacing: "0.2em",
              textTransform: "uppercase",
              fontWeight: 700,
              cursor: "pointer"
            }}
          >
            Try Again
          </button>
        </div>
      </body>
    </html>
  );
}
