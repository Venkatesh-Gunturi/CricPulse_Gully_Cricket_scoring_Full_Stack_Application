import "./Footer.css";

function Footer() {
  return (
    <footer className="cp-footer">
      <div className="cp-footer-inner">

        <div className="cp-footer-brand">
          <div className="cp-footer-logo">
            <span className="cp-footer-logo-ball">🏏</span>

            <span className="cp-footer-logo-text">
              Cric<span>Pulse</span>
            </span>
          </div>

          <p>
            Built with ❤️ for every cricket lover,
            from the streets to the scoreboard.
          </p>
        </div>

        <div className="cp-footer-socials">

          {/* Instagram */}
          <a
            href="https://instagram.com"
            target="_blank"
            rel="noreferrer"
            aria-label="Instagram"
          >
            <svg
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <rect
                x="3"
                y="3"
                width="18"
                height="18"
                rx="5"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              />

              <circle
                cx="12"
                cy="12"
                r="4"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
              />

              <circle
                cx="17.5"
                cy="6.5"
                r="1"
                fill="currentColor"
              />
            </svg>
          </a>

          {/* Facebook */}
          <a
            href="https://facebook.com"
            target="_blank"
            rel="noreferrer"
            aria-label="Facebook"
          >
            <svg
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path
                d="M14 8h3V4h-3c-3.31 0-5 1.69-5 5v3H6v4h3v4h4v-4h3l1-4h-4V9c0-.67.33-1 1-1Z"
                fill="currentColor"
              />
            </svg>
          </a>

          {/* X */}
          <a
            href="https://x.com"
            target="_blank"
            rel="noreferrer"
            aria-label="X"
          >
            <svg
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path
                d="M5 4h4.7l3.2 4.4L16.8 4H19l-5.1 6.1L19.5 20h-4.7l-3.6-4.9L7 20H4.8l5.5-6.6L5 4Zm3 2.1L15.9 18h.9L9 6.1H8Z"
                fill="currentColor"
              />
            </svg>
          </a>

          {/* LinkedIn */}
          <a
            href="https://linkedin.com"
            target="_blank"
            rel="noreferrer"
            aria-label="LinkedIn"
          >
            <svg
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path
                d="M6 8H3V21H6V8ZM4.5 3C3.67 3 3 3.67 3 4.5S3.67 6 4.5 6 6 5.33 6 4.5 5.33 3 4.5 3ZM21 13.5C21 9.91 19.09 8 16.25 8C14.88 8 13.95 8.61 13.5 9.23V8H10.5V21H13.5V14.5C13.5 12.78 13.83 11.5 15.45 11.5C17.05 11.5 17 13.03 17 14.6V21H20V13.5H21Z"
                fill="currentColor"
              />
            </svg>
          </a>

          {/* GitHub */}
            <a
            href="https://github.com"
            target="_blank"
            rel="noreferrer"
            aria-label="GitHub"
            >
            <svg
                viewBox="0 0 24 24"
                aria-hidden="true"
            >
                <path
                d="M12 2C6.48 2 2 6.58 2 12.24c0 4.52 2.87 8.36 6.84 9.72.5.1.68-.22.68-.49v-1.7c-2.78.62-3.37-1.37-3.37-1.37-.46-1.2-1.11-1.52-1.11-1.52-.91-.64.07-.63.07-.63 1 .08 1.53 1.06 1.53 1.06.9 1.58 2.35 1.12 2.92.86.09-.67.35-1.12.64-1.38-2.22-.26-4.55-1.14-4.55-5.05 0-1.12.39-2.03 1.02-2.75-.1-.26-.44-1.3.1-2.71 0 0 .83-.27 2.75 1.05A9.2 9.2 0 0 1 12 7.04c.84 0 1.68.12 2.47.36 1.92-1.32 2.75-1.05 2.75-1.05.54 1.41.2 2.45.1 2.71.63.72 1.02 1.63 1.02 2.75 0 3.92-2.34 4.78-4.57 5.04.36.32.68.94.68 1.9v2.78c0 .27.18.6.69.49A10.25 10.25 0 0 0 22 12.24C22 6.58 17.52 2 12 2Z"
                fill="currentColor"
                />
            </svg>
            </a>

          {/* Email */}
          <a
            href="mailto:"
            aria-label="Email"
          >
            <svg
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <path
                d="M3 5h18v14H3V5Zm2 2v.5l7 5 7-5V7l-7 5-7-5Z"
                fill="currentColor"
              />
            </svg>
          </a>

        </div>

        <div className="cp-footer-copy">
          © {new Date().getFullYear()} CricPulse.
          <span> Built for the local game.</span>
        </div>

      </div>
    </footer>
  );
}

export default Footer;