
import { Link } from 'react-router-dom';
import {
  FaFacebookF,
  FaInstagram,
  FaXTwitter,
  FaLinkedinIn,
  FaGithub,
  FaEnvelope,
  FaHeart,
  FaTrophy,
} from 'react-icons/fa6';
import { FaTelegramPlane } from 'react-icons/fa';

const socialLinks = [
  { label: 'Facebook', href: 'https://www.facebook.com/nani.nanna.946', Icon: FaFacebookF },
  { label: 'Instagram', href: 'https://www.instagram.com/nani_venky_404', Icon: FaInstagram },
  { label: 'X', href: 'https://x.com/Nani66035', Icon: FaXTwitter },
  { label: 'LinkedIn', href: 'https://www.linkedin.com/in/gunturi-venkatesh-82037b267', Icon: FaLinkedinIn },
  { label: 'GitHub', href: 'https://github.com/Venkatesh-Gunturi', Icon: FaGithub },
  { label: 'Email', href: 'mailto:nani66035@email.com', Icon: FaEnvelope },
  { label: 'Telegram', href: 'https://telegram.org/', Icon: FaTelegramPlane },
];

function Footer() {
  return (
    <footer className="site-footer mt-auto">
      <div className="container-fluid px-4 py-3">
        <div className="footer-compact">
          <div className="footer-identity">
            <Link to="/" className="footer-brand">
              <FaTrophy />
              <span>CricPulse</span>
            </Link>

            <p className="footer-description">
              Your local cricket. Your people. Your moments.
            </p>

            <p className="footer-love">
              Made with <FaHeart className="footer-heart" /> for cricket lovers.
            </p>
          </div>

          <div className="footer-community">
            <h2 className="footer-heading">Follow the game</h2>

            <div className="footer-socials">
              {socialLinks.map(({ label, href, Icon }) => (
                <a
                  key={label}
                  href={href}
                  aria-label={label}
                  title={label}
                  target={href.startsWith('mailto:') ? undefined : '_blank'}
                  rel={href.startsWith('mailto:') ? undefined : 'noreferrer'}
                >
                  <Icon />
                </a>
              ))}
            </div>
          </div>
        </div>

        <div className="footer-bottom">
          <span>© {new Date().getFullYear()} CricPulse</span>
          <span>Every ball. Every moment.</span>
        </div>
      </div>
    </footer>
  );
}

export default Footer;
